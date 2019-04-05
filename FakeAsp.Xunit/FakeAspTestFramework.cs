using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Hosting;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace FakeAsp.Xunit
{
    public class FakeAspTestFramework : XunitTestFramework
    {
        public FakeAspTestFramework(IMessageSink messageSink)
            : base(messageSink) {
        }

        protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
            => new FakeAspTestExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);
    }

    class FakeAspTestExecutor : XunitTestFrameworkExecutor
    {
        public FakeAspTestExecutor(AssemblyName assemblyName, ISourceInformationProvider sourceInformationProvider, IMessageSink diagnosticMessageSink) : base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
        { }

        protected override void RunTestCases(IEnumerable<IXunitTestCase> testCases, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions)
        {
            var proxy = CreateProxyHost();

            var domain = proxy.GetAppDomain();
            domain.SetData("sink", executionMessageSink);
            domain.SetData("cases", testCases.ToArray());
            domain.UnhandledException += Domain_UnhandledException;

            proxy.RunTestCases(TestAssembly, Serialize(testCases), executionMessageSink, executionOptions);

            AspHostProxy CreateProxyHost()
            {
                var baseDir = Path.GetDirectoryName(TestAssembly.Assembly.AssemblyPath);
                return (AspHostProxy)ApplicationHost.CreateApplicationHost(typeof(AspHostProxy), "/", baseDir);
            }
        }

        private void Domain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ad = (AppDomain)sender;
            var sink = (IMessageSink)ad.GetData("sink");
            var cases = (IXunitTestCase[])ad.GetData("cases");
            sink.OnMessage(new ErrorMessage(cases, (Exception)e.ExceptionObject));
        }

        string[] Serialize(IEnumerable<IXunitTestCase> testCases)
            => testCases.Select(c => XunitSerializationInfo.SerializeTriple(new XunitSerializationTriple("", c, c.GetType())))
                .ToArray();
    }

    public class AspHostProxy : MarshalByRefObject
    {
        public void RunTestCases(TestAssembly testAssembly, string[] testCaseStrings, IMessageSink messageSink, ITestFrameworkExecutionOptions options)
        {
            var runner = new XunitTestAssemblyRunner(
                            Deproxify(testAssembly), 
                            testCaseStrings.Select(Deserialize), 
                            messageSink, messageSink, options);
            runner.RunAsync().Ignore();
        }

        TestAssembly Deproxify(TestAssembly orig) {
            var assemblyInfo = new ReflectionAssemblyInfo(orig.Assembly.AssemblyPath);
            return new TestAssembly(assemblyInfo, orig.ConfigFileName, orig.Version);
        }

        IXunitTestCase Deserialize(string @string)
        {
            var triple = XunitSerializationInfo.DeserializeTriple(@string);
            return (IXunitTestCase)triple.Value;
        }

        public AppDomain GetAppDomain()
            => AppDomain.CurrentDomain;
    }

    public static class Extensions
    {
        public static void Ignore(this Task task) { }
    }
}
