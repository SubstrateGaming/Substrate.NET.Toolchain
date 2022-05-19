using Microsoft.AspNetCore.Http;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Ajuna.AspNetCore
{
   public class SubscriptionMiddleware
   {
      private readonly RequestDelegate _next;
      private SubscriptionHandlerBase _webSocketHandler { get; set; }

      public SubscriptionMiddleware(RequestDelegate next, SubscriptionHandlerBase webSocketHandler)
      {
         _next = next;
         _webSocketHandler = webSocketHandler;
      }

      public async Task InvokeAsync(HttpContext context)
      {
         if (!context.WebSockets.IsWebSocketRequest)
            return;

         var socket = await context.WebSockets.AcceptWebSocketAsync();
         await _webSocketHandler.OnConnectedAsync(socket);

         await ReceiveAsync(socket, async (result, buffer) =>
         {
            if (result.MessageType == WebSocketMessageType.Text)
            {
               await _webSocketHandler.OnReceivedAsync(socket, result, buffer);
            }

            else if (result.MessageType == WebSocketMessageType.Close)
            {
               await _webSocketHandler.OnDisconnectedAsync(socket);
            }
         });
      }

      private async Task ReceiveAsync(WebSocket socket, Func<WebSocketReceiveResult, byte[], Task> handleMessage)
      {
         var buffer = new byte[1024 * 4];

         while (socket.State == WebSocketState.Open)
         {
            var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer), cancellationToken: CancellationToken.None);
            await handleMessage(result, buffer);
         }
      }
   }
}
