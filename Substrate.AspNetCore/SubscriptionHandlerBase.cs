using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Substrate.AspNetCore
{
   public abstract class SubscriptionHandlerBase
   {
      protected SubscriptionManager Manager { get; set; }

      internal SubscriptionHandlerBase(SubscriptionManager subscriptionManager)
      {
         Manager = subscriptionManager;
      }

      internal virtual Task OnConnectedAsync(WebSocket socket)
      {
         Manager.AddSocket(socket);
         return Task.FromResult(0);
      }

      internal virtual async Task OnDisconnectedAsync(WebSocket socket)
      {
         string socketId = Manager.GetId(socket);
         if (!string.IsNullOrEmpty(socketId))
         {
            await Manager.RemoveSocketAsync(socketId);
            await DisconnectDelegateAsync(socket, socketId);
         }
      }

      internal virtual async Task OnReceivedAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
      {
         string socketId = Manager.GetId(socket);
         await ReceiveDelegateAsync(socket, socketId, result, buffer);
      }

      internal async Task SendMessageAsync(WebSocket socket, string message)
      {
         if (socket.State != WebSocketState.Open)
         {
            return;
         }

         await socket.SendAsync(buffer: new ArraySegment<byte>(array: Encoding.ASCII.GetBytes(message), offset: 0, count: message.Length), messageType: WebSocketMessageType.Text, endOfMessage: true, cancellationToken: CancellationToken.None);
      }

      internal async Task SendMessageAsync(string socketId, string message)
      {
         await SendMessageAsync(Manager.GetSocketById(socketId), message);
      }

      public abstract Task ReceiveDelegateAsync(WebSocket socket, string socketId, WebSocketReceiveResult result, byte[] buffer);

      public abstract Task DisconnectDelegateAsync(WebSocket socket, string socketId);
   }
}
