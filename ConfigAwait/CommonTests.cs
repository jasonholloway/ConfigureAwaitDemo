using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;
using System.Runtime.Remoting.Messaging;
using System.Web;
using FakeAsp;

namespace ConfigAwait
{
    public abstract class CommonTests<TRunner>
        where TRunner : IRunner, new()
    {
        [Fact]
        public Task HttpContext_AcrossAwaits()
            => Run(async () => {
                var ctx = HttpContext.Current;
                await Task.Delay(10);
                HttpContext.Current.ShouldBe(ctx);
            });

        [Fact]
        public Task IllogicalCallContext_AcrossAwaits()
            => Run(async () => {
                CallContext.SetData("hello", "bastard");
                await Task.Delay(10);
                CallContext.GetData("hello").ShouldBe("bastard");
            });

        [Fact]
        public Task LogicalCallContext_AcrossAwaits()
            => Run(async () => {
                CallContext.LogicalSetData("hello", "bastard");
                await Task.Delay(10);
                CallContext.LogicalGetData("hello").ShouldBe("bastard");
            });

        [Fact]
        public Task Awaiting()
            => Run(async () => {
                await AsyncMethod();
            });

        [Fact]
        public Task WaitingDirectlyForDelay()
            => Run(() => {
                Task.Delay(100).Wait();
            });

        [Fact]
        public Task BlockingOnTaskStartedOnOtherThread()
            => Run(() => {
                var task = new TaskFactory(TaskScheduler.Default)
                                .StartNew(NestedAwait)
                                .Unwrap();
                task.Wait();
            });

        [Fact]
        public Task BlockingOnTaskOnSameThread()
            => Run(() => {
                var task = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext())
                                .StartNew(NestedAwait)
                                .Unwrap();
                task.Wait();
            });

        [Fact]
        public Task Blocking_Wait()
            => Run(() => {
                AsyncMethod().Wait();
            });

        [Fact]
        public Task Blocking_Result()
            => Run(() => {
                var i = AsyncMethod().Result;
            });

        [Fact]
        public Task Blocking_AroundAwait()
            => Run(() => {
                Exec(async () => {
                    await AsyncMethod();
                }).Wait();
            });

        [Fact]
        public Task Blocking_AroundCompleteConfigAwait()
            => Run(() => {
                Exec(async () => {
                    await AsyncMethodCAF().ConfigureAwait(false);
                }).Wait();
            });

        [Fact]
        public Task Blocking_AroundIncompleteConfigAwait()
            => Run(() => {
                Exec(async () => {
                    await AsyncMethod().ConfigureAwait(false);
                }).Wait();
            });

        [Fact]
        public Task Blocking_AroundSynchronousAwait()
            => Run(() => {
                Exec(async () => {
                    Console.WriteLine("This will be completed synchronously!");
                }).Wait();
            });

        [Fact]
        public Task ConfigAwait_AboveOnly()
            => Run(async () => {
                await Exec(() => {
                    AsyncMethod().Wait();
                }).ConfigureAwait(false);
            });

        [Fact]
        public Task ConfigAwait_AboveAndBelow()
            => Run(async () => {
                await Exec(() => {
                    AsyncMethodCAF().Wait();
                }).ConfigureAwait(false);
            });

        [Fact]
        public Task PreliminaryConfigAwait()
            => Run(async () => {
                await Task.Delay(10).ConfigureAwait(false);
                await Exec(() => AsyncMethod().Wait());
                AsyncMethod().Wait();
            });


        async Task<int> AsyncMethod()
        {
            await Task.Delay(10);
            return 13;
        }

        async Task<int> AsyncMethodCAF()
        {
            await Task.Delay(10).ConfigureAwait(false);
            return 13;
        }

        static Task Exec(Func<Task> fn) => fn();
        static Task Exec(Action fn) => Exec(async () => fn());

        static async Task<int> NestedAwait() {
            await Task.Delay(50);
            return 13;
        }

        static async Task<int> NestedAwaitConfigAwait() {
            await Task.Delay(50).ConfigureAwait(false);
            return 13;
        }


        protected Test Run(Func<Task> fn)
            => new TRunner().Run(fn);

        protected Test Run(Action fn) 
            => Run(async () => fn());
    }
}