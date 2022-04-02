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
    public class AjunaOutputFormatter : TextOutputFormatter
    {
        public AjunaOutputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/ajuna"));
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        protected override bool CanWriteType(Type type)
        {
            return typeof(BaseType).IsAssignableFrom(type);
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var httpContext = context.HttpContext;

            var baseType = (BaseType)context.Object;
            await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(new { result = Utils.Bytes2HexString(baseType.Encode()) }), selectedEncoding);
        }
    }
}
