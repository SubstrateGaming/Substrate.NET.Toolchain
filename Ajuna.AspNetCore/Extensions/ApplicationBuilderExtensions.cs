using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Ajuna.AspNetCore
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSubscription(this IApplicationBuilder app, PathString path, SubscriptionHandler handler)
        {
            return app.Map(path, (_app) => _app.UseMiddleware<SubscriptionMiddleware>(handler));
        }
    }
}