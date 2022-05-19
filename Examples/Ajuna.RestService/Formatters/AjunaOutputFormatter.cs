using Ajuna.NetApi;
using Ajuna.NetApi.Model.Types.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Ajuna.RestService.Formatters
{
   /// <summary>
   /// >> AjunaOutputFormatter
   /// The AjunaOutputFormatter implements a custom formatter to easily encode any substrate type.
   /// Types are hex-encoded and uses the media type text/ajuna.
   /// </summary>
   public class AjunaOutputFormatter : TextOutputFormatter
   {
      /// <summary>
      /// Initializes the custom output formatter.
      /// </summary>
      public AjunaOutputFormatter()
      {
         SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/ajuna"));
         SupportedEncodings.Add(Encoding.UTF8);
         SupportedEncodings.Add(Encoding.Unicode);
      }

      /// <summary>
      /// Validates the given runtime type and checks whether it is assignable from BaseType class that is the base
      /// type of any substrate custom type.
      /// </summary>
      /// <param name="type">The given type to check against.</param>
      /// <returns>Returns true whether the requested type is formattable or not.</returns>
      protected override bool CanWriteType(Type type)
      {
         return typeof(BaseType).IsAssignableFrom(type);
      }

      /// <summary>
      /// Encodes and writes the given context object to the output stream.
      /// </summary>
      /// <param name="context">The given context.</param>
      /// <param name="selectedEncoding">The given encoding.</param>
      public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
      {
         var httpContext = context.HttpContext;
         var baseType = (BaseType)context.Object;
         await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(new { result = Utils.Bytes2HexString(baseType.Encode()) }), selectedEncoding);
      }
   }
}
