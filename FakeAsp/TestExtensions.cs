using System;
using System.Threading;
using System.Threading.Tasks;

namespace FakeAsp
{
    public static class TestExtensions
    {
        public static Test Timeout(this Test test, int timeout)
            => test.MapInner(fn => cancel => 
            {
                var x = FakeAsp.Context;

                Task.Run(() => DebuggingFriendlyTimer.Delay(timeout, cancel)
                                .ContinueWith(
                                    _ => x.OnError(new TimeoutException()), 
                                    TaskContinuationOptions.OnlyOnRanToCompletion));

                return fn(cancel);
            });

        public static Test Throws<Ex>(this Test test, string message = null)
            where Ex : Exception
            => test.MapInner(fn => async cancel => {
                var x = FakeAsp.Context;
                try
                {
                    await fn(cancel);
                    x.OnError(new Exception($"Didn't throw exception {typeof(Ex)}!"));
                }
                catch (Ex) {
                    x.OnComplete();
                }
            }); 
    }

    public class TimeoutException : Exception { }

    class DebuggingFriendlyTimer
    {
        public static async Task Delay(int ms, CancellationToken ct)
        {
            while(ms > 0)
            {
                await Task.Delay(30, ct);
                ms -= 30;
            }
        }
    }
}