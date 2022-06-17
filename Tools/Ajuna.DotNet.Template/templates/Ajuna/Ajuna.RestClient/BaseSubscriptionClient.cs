using Ajuna.ServiceLayer.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ajuna.RestClient
{
   public class BaseSubscriptionClient
   {
      private readonly ClientWebSocket _ws;

      public BaseSubscriptionClient(ClientWebSocket websocket)
      {
         _ws = websocket;
      }

      public BaseSubscriptionClient()
      {
         _ws = new ClientWebSocket();
      }

      public void Abort() => _ws.Abort();
      public Task ConnectAsync(Uri uri, CancellationToken cancellationToken) => _ws.ConnectAsync(uri, cancellationToken);
      public Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken) => _ws.CloseAsync(closeStatus, statusDescription, cancellationToken);

      public async Task<bool> SubscribeAsync(StorageSubscribeMessage message) => await SubscribeAsync(message, CancellationToken.None);

      public async Task<bool> SubscribeAsync(StorageSubscribeMessage message, CancellationToken cancellationToken)
      {
         byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
         await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken);

         // Get the confirmation
         string subscriptionResponse = await ReceiveMessageAsync(cancellationToken);
         if (string.IsNullOrWhiteSpace(subscriptionResponse))
         {
            return false;
         }

         // Parse the confirmation
         var subscriptionResponseResult = JsonConvert.DeserializeObject<StorageSubscribeMessageResult>(subscriptionResponse);
         if (subscriptionResponseResult == null)
         {
            return false;
         }

         // Ensure response status is expected
         return subscriptionResponseResult.Status == (int)HttpStatusCode.OK;
      }

      private async Task<string> ReceiveMessageAsync(CancellationToken cancellationToken)
      {
         byte[] buffer = new byte[ushort.MaxValue];
         WebSocketReceiveResult result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

         if (result.MessageType == WebSocketMessageType.Text && result.EndOfMessage)
         {
            return Encoding.UTF8.GetString(buffer, 0, result.Count);
         }

         await _ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationToken);
         return null;
      }
   }
}
