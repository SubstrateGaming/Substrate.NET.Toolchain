using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Ajuna.AspNetCore.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSubscription(this IApplicationBuilder app, PathString path, SubscriptionHandlerBase handler)
        {
            return app.Map(path, (_app) => _app.UseMiddleware<SubscriptionMiddleware>(handler));
        }
    }
}