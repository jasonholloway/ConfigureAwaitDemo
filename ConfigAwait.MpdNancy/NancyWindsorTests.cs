using ConfigAwait;
using FakeAsp;
using MPD.Core.Infrastructure.Api;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Windsor;
using System;
using System.Threading.Tasks;
using System.Web;
using Xunit;

namespace ConfigAwait.MpdNancy
{
    public class NancyWindsorTests : CommonTests<NancyWindsorRunner>
    {
        [Fact]
        public void Hello() { }
    }


    public class Bootstrapper : MpdNancyBootstrapper
    {

    }


    public class NancyWindsorRunner : IRunner
    {
        static NancyWindsorRunner()
        {
            NancyBootstrapperLocator.Bootstrapper = new Bootstrapper();
        }

        public Test Run(Func<Task> fn)
            => FakeAsp.FakeAsp.Run(() => Task.CompletedTask);
    }

}
