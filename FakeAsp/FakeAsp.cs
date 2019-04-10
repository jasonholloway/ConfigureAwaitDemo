using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using static FakeAsp.Test;

namespace FakeAsp
{
    public static class FakeAsp
    {
        [ThreadStatic]
        static FakeAspContext _context;

        public static FakeAspContext Context => _context;

        static FakeAsp()
        {
            HttpApplication.RegisterModule(typeof(ErrorCatcher));
        }

        public static Test RunAsp(FnInner fn)
            => Create(fn)
                .MapOuter(_ => RunArbitraryFn)
                .Timeout(300);

        public static Test RunAsp(Func<Task> fn)
            => RunAsp(_ => fn());

        public static Test RunAsp(Action fn)
            => RunAsp(async () => fn());

        static Task RunArbitraryFn(CancellationToken cancel, FnInner fn)
        {
            var tcs = new TaskCompletionSource<bool>();
            try
            {
                var thread = new Thread(new ThreadStart(() =>
                {
                    try
                    {
                        _context = new FakeAspContext(tcs, fn, cancel);
                        HttpRuntime.ProcessRequest(new SimpleWorkerRequest("", "", new StringWriter()));
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
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
                tcs.TrySetException(ex);
            }

            return tcs.Task;
        }
    }

    public class FakeAspContext
    {
        readonly TaskCompletionSource<bool> _tcs;
        readonly FnInner _fn;
        readonly CancellationToken _cancel;

        public FakeAspContext(TaskCompletionSource<bool> tcs, FnInner fn, CancellationToken cancel)
        {
            _tcs = tcs;
            _fn = fn;
            _cancel = cancel;
        }

        public void OnError(Exception ex)
            => _tcs.TrySetException(ex);

        public void OnComplete()
            => _tcs.TrySetResult(true);

        public Task RunFn()
            => _fn(_cancel);
    }

}