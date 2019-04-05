using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace FakeAsp
{
    public static class FakeAsp
    {
        [ThreadStatic]
        public static (TaskCompletionSource<bool>, Func<CancellationToken, Task>, CancellationToken) Context;

        static FakeAsp()
        {
            HttpApplication.RegisterModule(typeof(ErrorCatcher));
        }

        public static Test RunAsp(Func<Task> fn)
            => RunAsp(_ => fn());

        public static Test RunAsp(Func<CancellationToken, Task> fn)
            => new Test(cancel => {
                var tcs = new TaskCompletionSource<bool>();
                try
                {
                    var thread = new Thread(new ThreadStart(() =>
                    {
                        try
                        {
                            Context = (tcs, fn, cancel);
                            HttpRuntime.ProcessRequest(new SimpleWorkerRequest("", "", new StringWriter()));
                        }
                        catch (Exception ex)
                        {
                            tcs.SetException(ex);
                        }
                    }));

                    thread.Start();

                    cancel.Register(() =>
                    {
                        thread.Abort();
                        tcs.TrySetException(new Exception("Cancelled!"));
                    });
                }
                catch(Exception ex)
                {
                    tcs.SetException(ex);
                }

                return tcs.Task;
            });
    }
}