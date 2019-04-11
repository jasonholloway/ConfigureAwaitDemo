using ConfigAwait;
using FakeAsp;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ConfigAwait.Nancy
{
    public class NancyWindsorTests : CommonTests<NancyWindsorRunner>
    {
        [Fact]
        public void Hello() { }
    }


    public class NancyWindsorRunner : IRunner
    {
        public Test Run(Func<Task> fn)
        {
            throw new NotImplementedException();
        }
    }
}
