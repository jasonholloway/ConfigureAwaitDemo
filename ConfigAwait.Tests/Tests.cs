using Shouldly;
using System;
using System.Threading.Tasks;
using System.Web;
using Xunit;
using System.Runtime.Remoting.Messaging;
using static FakeAsp.FakeAsp;

namespace ConfigAwait.TaskFriendly
{
    public class Tests
    {
        public class Flow
        {
            [Fact]
            public Task HttpContext_AcrossAwaits()
                => RunAsp(async () => {
                    var ctx = HttpContext.Current;
                    await Task.Delay(10);
                    HttpContext.Current.ShouldBe(ctx);
                });

            [Fact]
            public Task IllogicalCallContext_AcrossAwaits()
                => RunAsp(async () => {
                    CallContext.SetData("hello", "bastard");
                    await Task.Delay(10);
                    CallContext.GetData("hello").ShouldBe("bastard");
                });

            [Fact]
            public Task LogicalCallContext_AcrossAwaits()
                => RunAsp(async () => {
                    CallContext.LogicalSetData("hello", "bastard");
                    await Task.Delay(10);
                    CallContext.LogicalGetData("hello").ShouldBe("bastard");
                });
        }

        public class BlockingBehaviour
        {
            [Fact]
            public Task Awaiting()
                => RunAsp(async () => {
                    await AsyncMethod();
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
                                    .StartNew(NestedAwait)
                                    .Unwrap();
                    task.Wait();
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
            public Task Blocking_AroundAwait()
                => RunAsp(() => {
                    Exec(async () => {
                        await AsyncMethod();
                    }).Wait();
                });

            [Fact]
            public Task Blocking_AroundCompleteConfigAwait()
                => RunAsp(() => {
                    Exec(async () => {
                        await AsyncMethodCAF().ConfigureAwait(false);
                    }).Wait();
                });

            [Fact]
            public Task Blocking_AroundIncompleteConfigAwait()
                => RunAsp(() => {
                    Exec(async () => {
                        await AsyncMethod().ConfigureAwait(false);
                    }).Wait();
                });

            [Fact]
            public Task Blocking_AroundSynchronousAwait()
                => RunAsp(() => {
                    Exec(async () => {
                        Console.WriteLine("This will be completed synchronously!");
                    }).Wait();
                });

            [Fact]
            public Task ConfigAwait_AboveOnly()
                => RunAsp(async () => {
                    await Exec(() => {
                        AsyncMethod().Wait();
                    }).ConfigureAwait(false);
                });

            [Fact]
            public Task ConfigAwait_AboveAndBelow()
                => RunAsp(async () => {
                    await Exec(() => {
                        AsyncMethodCAF().Wait();
                    }).ConfigureAwait(false);
                });

            [Fact]
            public Task PreliminaryConfigAwait()
                => RunAsp(async () => {
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