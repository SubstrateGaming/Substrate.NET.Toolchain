using Microsoft.AspNetCore.Http;
using Serilog;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Substrate.AspNetCore
{
   /// <summary>
   /// Middleware for handling the Web Socket Subscriptions
   /// </summary>
   public class SubscriptionMiddleware : IMiddleware
   {
      private StorageSubscriptionHandler WebSocketHandler { get; set; }

      public SubscriptionMiddleware(StorageSubscriptionHandler webSocketHandler)
      {
         WebSocketHandler = webSocketHandler;
      }

      public async Task InvokeAsync(HttpContext context, RequestDelegate next)
      {
         if (!context.WebSockets.IsWebSocketRequest)
         {
            return;
         }

         WebSocket socket = await context.WebSockets.AcceptWebSocketAsync();
         await WebSocketHandler.OnConnectedAsync(socket);

         try
         {
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
         catch (Exception ex)
         {
            // Ignored
            Log.Warning(ex, "could not handle websocket");
         }

         await WebSocketHandler.OnDisconnectedAsync(socket);
      }

      private async Task ReceiveAsync(WebSocket socket, Func<WebSocketReceiveResult, byte[], Task> handleMessage)
      {
         byte[] buffer = new byte[ushort.MaxValue];

         while (socket.State == WebSocketState.Open)
         {
            WebSocketReceiveResult result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer), cancellationToken: CancellationToken.None);
            await handleMessage(result, buffer);
         }
      }
   }
}
