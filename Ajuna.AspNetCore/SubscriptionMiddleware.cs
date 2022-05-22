using Microsoft.AspNetCore.Http;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Ajuna.AspNetCore
{
   public class SubscriptionMiddleware
   {
      private SubscriptionHandlerBase WebSocketHandler { get; set; }

      public SubscriptionMiddleware(SubscriptionHandlerBase webSocketHandler)
      {
         WebSocketHandler = webSocketHandler;
      }

      public async Task InvokeAsync(HttpContext context)
      {
         if (!context.WebSockets.IsWebSocketRequest)
         {
            return;
         }

         WebSocket socket = await context.WebSockets.AcceptWebSocketAsync();
         await WebSocketHandler.OnConnectedAsync(socket);

         await ReceiveAsync(socket, async (result, buffer) =>
         {
            if (result.MessageType == WebSocketMessageType.Text)
            {
               await WebSocketHandler.OnReceivedAsync(socket, result, buffer);
            }

            else if (result.MessageType == WebSocketMessageType.Close)
            {
               await WebSocketHandler.OnDisconnectedAsync(socket);
            }
         });
      }

      private async Task ReceiveAsync(WebSocket socket, Func<WebSocketReceiveResult, byte[], Task> handleMessage)
      {
         byte[] buffer = new byte[1024 * 4];

         while (socket.State == WebSocketState.Open)
         {
            WebSocketReceiveResult result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer), cancellationToken: CancellationToken.None);
            await handleMessage(result, buffer);
         }
      }
   }
}
