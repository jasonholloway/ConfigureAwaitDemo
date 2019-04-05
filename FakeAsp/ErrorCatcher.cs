using System.Web;

namespace FakeAsp
{
    public class ErrorCatcher : IHttpModule
    {
        public void Init(HttpApplication context)
            => context.Error += (_, args) => {
                var (tcs, _, __) = FakeAsp.Context;
                tcs.TrySetException(HttpContext.Current.Error);
            };

        public void Dispose()
        { }
    }

}