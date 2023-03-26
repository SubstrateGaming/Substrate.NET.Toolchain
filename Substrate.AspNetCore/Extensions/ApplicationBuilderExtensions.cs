using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Substrate.AspNetCore.Extensions
{
   public static class ApplicationBuilderExtensions
   {
      public static IApplicationBuilder UseSubscription(this IApplicationBuilder app, PathString path)
      {
         return app.Map(path, (_app) => _app.UseMiddleware<SubscriptionMiddleware>());
      }
   }
}