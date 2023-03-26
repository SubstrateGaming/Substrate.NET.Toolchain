using Substrate.ServiceLayer.Extensions;
using Substrate.ServiceLayer.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.AspNetCore
{
   public class StorageSubscriptionHandler : SubscriptionHandlerBase
   {
      private static readonly object _subscriptionsLock = new object();
      private readonly Dictionary<string, List<string>> _subscriptions = new Dictionary<string, List<string>>();

      public StorageSubscriptionHandler(SubscriptionManager subscriptionManager) : base(subscriptionManager)
      {
      }

      /// <summary>
      ///  Called on every Websocket call coming from the client
      /// </summary>
      /// <param name="socket"></param>
      /// <param name="socketId"></param>
      /// <param name="result"></param>
      /// <param name="buffer"></param>
      public override async Task ReceiveDelegateAsync(WebSocket socket, string socketId, WebSocketReceiveResult result, byte[] buffer)
      {
         if (!result.EndOfMessage || buffer == null || buffer.Length == 0)
         {
            await OnDisconnectedAsync(socket);
         }

         try
         {
            StorageSubscribeMessage command = JsonConvert.DeserializeObject<StorageSubscribeMessage>(Encoding.UTF8.GetString(buffer));
            if (command == null)
            {
               // Close the socket because the message is not interpreted.
               await OnDisconnectedAsync(socket);
            }

            // Register the subscription.
            if (RegisterSubscription(socketId, command))
            {
               // Confirm the subscription.
               await SendMessageAsync(socket, JsonConvert.SerializeObject(new StorageSubscriptionMessage()
               {
                  Type = StorageSubscriptionMessageType.StorageSubscribeMessageResult,
                  Payload = JsonConvert.SerializeObject(new StorageSubscribeMessageResult()
                  {
                     Status = (int)HttpStatusCode.OK
                  })
               }));

               return;
            }

            await SendMessageAsync(socket, JsonConvert.SerializeObject(new StorageSubscriptionMessage()
            {
               Type = StorageSubscriptionMessageType.StorageSubscribeMessageResult,
               Payload = JsonConvert.SerializeObject(new StorageSubscribeMessageResult()
               {
                  Status = (int)HttpStatusCode.BadRequest
               })
            }));

            return;
         }
         catch (Exception)
         {
            // Close the socket because the message is not interpreted.
            await OnDisconnectedAsync(socket);
         }
      }


      public override Task DisconnectDelegateAsync(WebSocket socket, string clientId)
      {
         RemoveSubscriptionsForClient(clientId);
         return Task.FromResult(0);
      }

      internal void BroadcastChange(string identifier, string key, string data, StorageSubscriptionChangeType changeType)
      {
         // Get all subscriptions
         string[] clients = GetSubscribedSockets(identifier, key);
         if (clients.Length == 0)
         {
            return;
         }

         // Format the change notification that is send to all subscribed clients
         string message = FormatMessage(identifier, key, data, changeType);
         if (string.IsNullOrEmpty(message))
         {
            return;
         }

         // Retreive all subscribed clients
         var sockets = Manager
             .GetAll()
             .Where(x => clients.Contains(x.Key))
             .Select(x => x.Value)
             .ToList();

         if (sockets.Count == 0)
         {
            return;
         }

         // Publish the message to all connected clients
         sockets.ForEachAsync(Environment.ProcessorCount, async (WebSocket socket) => { await SendMessageAsync(socket, message); });
      }

      private string FormatMessage(string identifier, string key, string data, StorageSubscriptionChangeType changeType)
      {
         return JsonConvert.SerializeObject(new StorageSubscriptionMessage()
         {
            Type = StorageSubscriptionMessageType.StorageChangeMessage,
            Payload = JsonConvert.SerializeObject(new StorageChangeMessage()
            {
               Type = changeType,
               Identifier = identifier,
               Key = key,
               Data = data,
               Timestamp = DateTime.UtcNow
            })
         });
      }

      private string GetSubscriptionKey(StorageSubscribeMessage command) => GetSubscriptionKey(command.Identifier, command.Key);
      private string GetSubscriptionKey(string identifier, string key)
      {
         if (string.IsNullOrEmpty(key))
         {
            return identifier.ToLowerInvariant();
         }

         return $"{identifier.ToLowerInvariant()}/{key.ToLowerInvariant()}";
      }

      private string[] GetSubscribedSockets(string identifier, string key)
      {
         var result = new List<string>();
         string subscriptionKeyExact = GetSubscriptionKey(identifier, key);
         string subscriptionKeyAny = GetSubscriptionKey(identifier, string.Empty);

         lock (_subscriptionsLock)
         {
            foreach (KeyValuePair<string, List<string>> kvp in _subscriptions)
            {
               if (kvp.Value.Any(x => x == subscriptionKeyExact || x == subscriptionKeyAny))
               {
                  result.Add(kvp.Key);
               }
            }
         }

         return result.Distinct().ToArray();
      }

      private bool RegisterSubscription(string clientId, StorageSubscribeMessage command)
      {
         if (string.IsNullOrEmpty(clientId))
         {
            return false;
         }

         if (string.IsNullOrEmpty(command.Identifier))
         {
            return false;
         }

         string subscriptionKey = GetSubscriptionKey(command);

         lock (_subscriptionsLock)
         {
            if (_subscriptions.ContainsKey(clientId))
            {
               List<string> subscriptionsForClient = _subscriptions[clientId];
               if (subscriptionsForClient.Contains(subscriptionKey))
               {
                  // Client is already registered
                  return true;
               }
               else
               {
                  // Client registered to the given key.
                  subscriptionsForClient.Add(subscriptionKey);
                  return true;
               }
            }

            // Client registered its first subscription.
            _subscriptions.Add(clientId, new List<string>() { subscriptionKey });
            return true;
         }
      }

      private void RemoveSubscriptionsForClient(string clientId)
      {
         lock (_subscriptionsLock)
         {
            if (_subscriptions.ContainsKey(clientId))
            {
               _subscriptions.Remove(clientId);
            }
         }
      }
   }
}