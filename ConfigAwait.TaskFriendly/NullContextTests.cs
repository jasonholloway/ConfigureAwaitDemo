using FakeAsp;
using System;
using System.Threading.Tasks;

namespace ConfigAwait.TaskFriendly
{
    public class NullContextTests : CommonTests<NullContextRunner>
    {
    }

    public class NullContextRunner : IRunner
    {
        public Test Run(Func<Task> fn)
            => FakeAsp.FakeAsp.Run(async () => 
            {
                await Task.Delay(10).ConfigureAwait(false); //sheds off SynchronizationContext
                await fn();
            });
    }
}
