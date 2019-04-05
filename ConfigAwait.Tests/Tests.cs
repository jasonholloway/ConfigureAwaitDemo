using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Xunit;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using static FakeAsp.FakeAsp;
using FakeAsp;

namespace ConfigAwait.Tests
{
    public class Tests
    {
        public Tests()
        {
            FakeNancyAsp.Setup();
        }

        [Fact]
        public Task HasAspSyncContext()
            => RunAsp(async () => {
                SynchronizationContext.Current.ShouldBeOfType(Types.AspNetSynchronizationContext);
            });

        [Fact]
        public Task HasAspSyncContext_AfterAwait()
            => RunAsp(async () => {
                await Task.Delay(10);
                SynchronizationContext.Current.ShouldBeOfType(Types.AspNetSynchronizationContext);
            });

        [Fact]
        public Task ExceptionPropagates()
            => RunAsp(async () => {
                throw new DummyException();
            }).Throws<DummyException>();

        [Fact]
        public Task ExceptionPropagates_AfterAwait()
            => RunAsp(async () => {
                await Task.Delay(10);
                throw new DummyException();
            }).Throws<DummyException>();

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
        public Task Deadlocks_OnBlockingNestedAwait()
            => RunAsp(async ct => {
                TraceThread("TestStart");
                NestedAwait().Wait(ct);
            }).Deadlocks();

        [Fact]
        public Task Deadlocks_OnBlockingNestedAwaitResult()
            => RunAsp(async () => {
                var i = NestedAwait().Result;
            }).Deadlocks();

        [Fact]
        public Task DoesntDeadlock_WaitingDirectlyForDelay()
            => RunAsp(async () => {
                Task.Delay(100).Wait();
            });

        [Fact]
        public Task DoesntDeadlock_BlockingOnTaskStartedOnOtherThread()
            => RunAsp(async () => {
                TraceThread("TestStart");
                var task = new TaskFactory(TaskScheduler.Default)
                                .StartNew(NestedAwait)
                                .Unwrap();
                task.Wait();
            });

        [Fact]
        public Task Deadlocks_BlockingOnTaskOnSameThread()
            => RunAsp(async () => {
                TraceThread("TestStart");
                var task = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext())
                                .StartNew(() => NestedAwait())
                                .Unwrap();
                task.Wait();
            }).Deadlocks();


        class DummyException : Exception { }

        async Task<int> NestedAwait() {
            TraceThread("NestedAwait before");
            await Task.Delay(50);
            TraceThread("NestedAwait after");
            return 13;
        }

        static void TraceThread(string tag)
        {
            Trace.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: {tag}");
            Trace.Flush();
        }
    }

    public static class Types
    {
        public static Type AspNetSynchronizationContext = Type.GetType("System.Web.AspNetSynchronizationContext, System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
    }
}