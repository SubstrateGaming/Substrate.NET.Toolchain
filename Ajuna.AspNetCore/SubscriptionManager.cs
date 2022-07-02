using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Ajuna.AspNetCore
{
   public class SubscriptionManager
   {
      private ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();

      public WebSocket GetSocketById(string id)
      {
         return _sockets.FirstOrDefault(p => p.Key == id).Value;
      }

      public ConcurrentDictionary<string, WebSocket> GetAll()
      {
         return _sockets;
      }

      public string GetId(WebSocket socket)
      {
         return _sockets.FirstOrDefault(p => p.Value == socket).Key;
      }

      public void AddSocket(WebSocket socket)
      {
         _sockets.TryAdd(CreateConnectionId(), socket);
      }

      public async Task RemoveSocketAsync(string id)
      {
         WebSocket socket;
         if (_sockets.TryRemove(id, out socket))
         {
            if (socket.State == WebSocketState.Open)
            {
               await socket.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure, statusDescription: "Closed", cancellationToken: CancellationToken.None);
            }
            else
            {
               socket.Abort();
            }
         }
      }

      private string CreateConnectionId()
      {
         return Guid.NewGuid().ToString();
      }
   }
}