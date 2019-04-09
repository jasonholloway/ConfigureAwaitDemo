using Shouldly;
using System;
using System.Threading.Tasks;
using System.Web;
using Xunit;
using System.Runtime.Remoting.Messaging;
using static FakeAsp.FakeAsp;
using FakeAsp;

namespace ConfigAwait.TaskFriendly
{
    public class Tests
    {
        [Fact]
        public Task RetainsHttpContext_AcrossAwaits()
            => RunAsp(async () => {
                var ctx = HttpContext.Current;
                await Task.Delay(10);
                HttpContext.Current.ShouldBe(ctx);
            });

        [Fact]
        public Task RetainsCallContext_AcrossAwaits()
            => RunAsp(async () => {
                CallContext.LogicalSetData("hello", "bastard");
                await Task.Delay(10);
                CallContext.LogicalGetData("hello").ShouldBe("bastard");
            });

        [Fact]
        public Task WaitingDirectlyForDelay()
            => RunAsp(async () => {
                Task.Delay(100).Wait();
            });

        [Fact]
        public Task BlockingOnTaskStartedOnOtherThread()
            => RunAsp(async () => {
                var task = new TaskFactory(TaskScheduler.Default)
                                .StartNew(NestedAwait)
                                .Unwrap();
                task.Wait();
            });

        [Fact]
        public Task BlockingOnTaskOnSameThread()
            => RunAsp(async () => {
                var task = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext())
                                .StartNew(() => NestedAwait())
                                .Unwrap();
                task.Wait();
            });


        public class OnCallingAsyncMethod
        {
            [Fact]
            public Task AwaitingWorksFine()
                => RunAsp(async () => { 
                    await AsyncMethod();
                });

            [Fact]
            public Task BlockingWait()
                => RunAsp(async () => {
                    AsyncMethod().Wait();
                });

            [Fact]
            public Task BlockingResult()
                => RunAsp(async () => {
                    var i = AsyncMethod().Result;
                });

            [Fact]
            public Task InnerConfigAwaitFixes()
                => RunAsp(async () => {
                    AsyncMethodWithConfigAwait().Wait();
                });

            [Fact]
            public Task OuterConfigAwaitNotEnough()
                => RunAsp(async () => {
                    await Exec(async () => {
                        AsyncMethod().Wait();
                    }).ConfigureAwait(false);
                }).Deadlocks();


            async Task<int> AsyncMethod()
            {
                await Task.Delay(10);
                return 13;
            }

            async Task AsyncMethodWithConfigAwait()
                => await Task.Delay(10).ConfigureAwait(false);
        }

        static Task Exec(Func<Task> fn) => fn();


        static async Task<int> NestedAwait() {
            await Task.Delay(50);
            return 13;
        }

        static async Task<int> NestedAwaitConfigAwait() {
            await Task.Delay(50).ConfigureAwait(false);
            return 13;
        }

    }
}