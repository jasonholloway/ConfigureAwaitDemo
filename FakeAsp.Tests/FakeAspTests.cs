using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using System.Diagnostics;
using static FakeAsp.FakeAsp;
using FakeAsp.Nancy;

namespace FakeAsp.Tests
{
    public class FakeAspTests
    {
        public FakeAspTests()
        {
            FakeNancyAsp.Setup();
        }

        [Fact]
        public Task HasAspSyncContext()
            => Run(async () => {
                SynchronizationContext.Current.ShouldBeOfType(Types.AspNetSynchronizationContext);
            });

        [Fact]
        public Task HasAspSyncContext_AfterAwait()
            => Run(async () => {
                await Task.Delay(10);
                SynchronizationContext.Current.ShouldBeOfType(Types.AspNetSynchronizationContext);
            });

        [Fact]
        public Task ExceptionPropagates()
            => Run(async () => {
                throw new DummyException();
            }).Throws<DummyException>();

        [Fact]
        public Task ExceptionPropagates_AfterAwait()
            => Run(async () => {
                await Task.Delay(10);
                throw new DummyException();
            }).Throws<DummyException>();
        static void TraceThread(string tag)
        {
            Trace.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: {tag}");
            Trace.Flush();
        }
    }

    public static class Types
    {
        public static Type AspNetSynchronizationContext = Type.GetType("System.Web.AspNetSynchronizationContext, System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
    }
}