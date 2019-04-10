using System;
using System.Threading;
using System.Threading.Tasks;

namespace FakeAsp
{
    public class Test
    {
        public delegate Task FnInner(CancellationToken cancel);
        public delegate Task FnOuter(CancellationToken cancel, FnInner fnInner);

        readonly FnOuter _fnOuter;
        readonly FnInner _fnInner;

        private Test(FnOuter fnOuter, FnInner fnInner)
        {
            _fnOuter = fnOuter;
            _fnInner = fnInner;
        }

        public Task Run(CancellationToken cancel)
            => _fnOuter(cancel, _fnInner);

        public static implicit operator Task(Test test)
            => test.Run(CancellationToken.None);


        public static Test Create(FnInner fn) => new Test((ct, fnInner) => fnInner(ct), fn);
        public static Test Empty = Create(_ => Task.FromResult(true));

        public Test MapInner(Func<FnInner, FnInner> fn)
            => new Test(_fnOuter, fn(_fnInner));

        public Test MapOuter(Func<FnOuter, FnOuter> fn)
            => new Test(fn(_fnOuter), _fnInner);
    }

}