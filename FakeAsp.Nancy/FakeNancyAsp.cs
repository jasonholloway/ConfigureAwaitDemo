using Nancy;
using Nancy.Bootstrapper;
using Nancy.Hosting.Aspnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static FakeAsp.FakeAsp;

namespace FakeAsp.Nancy
{
    public static class FakeNancyAsp
    {
        public static Test NancyAsp(Func<Task> fn)
            => new Test(cancel => {
                return RunAsp(fn);
            });

        static FakeNancyAsp()
        {
            Setup();
        }

        public static void Setup()
        {
            NancyBootstrapperLocator.Bootstrapper = new Bootstrapper();
        }

        public class ShimModule : NancyModule
        {
            public ShimModule()
            {
                Get["/", true] = async (_, __) =>
                {
                    var x = FakeAsp.Context;
                    try
                    {
                        await x.RunFn();
                        x.OnComplete();
                    }
                    catch(Exception ex)
                    {
                        x.OnError(ex);
                    }

                    return "hello!";
                };
            }
        }

        public class Bootstrapper : DefaultNancyAspNetBootstrapper
        {
            protected override IEnumerable<ModuleRegistration> Modules
                => base.Modules.Concat(new[] { new ModuleRegistration(typeof(ShimModule)) });

            public override INancyModule GetModule(Type moduleType, NancyContext context)
            {
                if (moduleType == typeof(ShimModule))
                    return new ShimModule();
                else
                    return base.GetModule(moduleType, context);
            }
        }

    }


}