using System;
using System.Threading.Tasks;

namespace FakeAsp
{
    public interface IRunner
    {
        Test Run(Func<Task> fn);
    }
}
