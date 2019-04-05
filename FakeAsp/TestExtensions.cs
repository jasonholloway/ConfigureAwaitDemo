using Shouldly;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FakeAsp
{
    public static class TestExtensions
    {
        public static Test Timeout(this Test test, int timeout)
            => new Test(async cancel => {
                var timerCancelSource = new CancellationTokenSource();
                var comboCancelSource = CancellationTokenSource.CreateLinkedTokenSource(cancel, timerCancelSource.Token);
                comboCancelSource.Token.Register(() => Trace.WriteLine("Cancelling!"));

                var timer = Task.Run(() => Task.Delay(timeout));
                var completed = await Task.WhenAny(timer, test.Run(comboCancelSource.Token));

                timerCancelSource.Cancel();
                if (completed == timer) throw new TimeoutException();
            });

        class TimeoutException : Exception { }


        public static Test Throws<Ex>(this Test test, string message = null)
            where Ex : Exception
            => new Test(cancel => 
                    Should.ThrowAsync(test.Run(cancel), message, typeof(Ex)));

        public static Test Deadlocks(this Test test, int timeout = 5000)
            => test.Timeout(timeout)
                .Throws<TimeoutException>("Deadlock expected!");
    }

}