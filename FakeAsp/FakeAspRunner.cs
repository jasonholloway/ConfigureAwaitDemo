using System;
using System.Threading.Tasks;
using FakeAsp;

namespace FakeAsp
{
    public class FakeAspRunner : IRunner
    {
        public Test Run(Func<Task> fn)
            => FakeAsp.Run(fn);
    }
}
