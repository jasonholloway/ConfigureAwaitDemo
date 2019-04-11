using System;
using System.Threading.Tasks;
using FakeAsp;

namespace ConfigAwait.Tests.TaskFriendly
{
    public class FakeAspRunner : IRunner
    {
        public Test Run(Func<Task> fn)
            => FakeAsp.FakeAsp.Run(fn);
    }
}
