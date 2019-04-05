using System.Web;

namespace FakeAsp
{
    public class ErrorCatcher : IHttpModule
    {
        public void Init(HttpApplication context)
            => context.Error += (_, args) => {
                FakeAsp.Context.OnError(context.Context.Error);
            };

        public void Dispose()
        { }
    }

}