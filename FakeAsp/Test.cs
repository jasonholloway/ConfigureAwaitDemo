using System;
using System.Threading;
using System.Threading.Tasks;

namespace FakeAsp
{
    public class Test
    {
        readonly Func<CancellationToken, Task> _fn;

        public Test(Func<CancellationToken, Task> fn)
        {
            _fn = fn;
        }

        public Task Run(CancellationToken cancel)
            => _fn(cancel);

        public static implicit operator Task(Test test)
            => test.Run(CancellationToken.None);
    }

}