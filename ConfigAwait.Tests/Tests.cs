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
            public Task Awaiting()
                => RunAsp(async () => { 
                    await AsyncMethod();
                });

            [Fact]
            public Task Blocking_Wait()
                => RunAsp(() => {
                    AsyncMethod().Wait();
                });

            [Fact]
            public Task Blocking_Result()
                => RunAsp(() => {
                    var i = AsyncMethod().Result;
                });

            [Fact]
            public Task InnerConfigAwaitFixes()
                => RunAsp(async () => {
                    AsyncMethodWithConfigAwait().Wait();
                });

            [Fact]
            public Task InnerConfigAwaitFixes2()
                => RunAsp(async () => {
                    Exec(async () => await Task.Delay(10).ConfigureAwait(false)).Wait();
                });

            //[Fact]
            //public Task InnerConfigAwaitFixes3()
            //    => RunAsp(async () => {
            //        Exec(async () => await AsyncMethod().ConfigureAwait(false)).Wait();
            //    });

            [Fact]
            public Task Blocking_AboveConfigAwait()
                => RunAsp(async () => {
                    Exec(async () => {
                        await AsyncMethod().ConfigureAwait(false);
                    }).Wait();
                });

            [Fact]
            public Task ConfigAwait_AboveBlocking()
                => RunAsp(async () => {
                    await Exec(() => {
                        AsyncMethod().Wait();
                    }).ConfigureAwait(false);
                });


            async Task<int> AsyncMethod()
            {
                await Task.Delay(10);
                return 13;
            }

            async Task AsyncMethodWithConfigAwait()
                => await Task.Delay(10).ConfigureAwait(false);
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

    }
}