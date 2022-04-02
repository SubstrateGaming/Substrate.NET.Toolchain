using Ajuna.ServiceLayer.Extensions;
using Ajuna.ServiceLayer.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Ajuna.AspNetCore
{
    public class StorageSubscriptionHandler : SubscriptionHandlerBase
    {
        private static object _subscriptionsLock = new object();
        private Dictionary<string, List<string>> _subscriptions = new Dictionary<string, List<string>>();

        public StorageSubscriptionHandler(SubscriptionManager subscriptionManager) : base(subscriptionManager)
        {
        }

        public override Task ReceiveDelegateAsync(WebSocket socket, string socketId, WebSocketReceiveResult result, byte[] buffer)
        {
            if (!result.EndOfMessage || buffer == null || buffer.Length == 0)
            {
                OnDisconnectedAsync(socket).ConfigureAwait(false);
                return Task.FromResult(0);
            }

            try
            {
                var command = JsonConvert.DeserializeObject<StorageSubscribeMessage>(Encoding.UTF8.GetString(buffer));
                if (command == null)
                {
                    // Close the socket because the message is not interpreted.
                    OnDisconnectedAsync(socket).ConfigureAwait(false);
                    return Task.FromResult(0);
                }

                // Register the subscription.
                if (RegisterSubscription(socketId, command))
                {
                    // Confirm the subscription.
                    return SendMessageAsync(socket, JsonConvert.SerializeObject(new StorageSubscribeMessageResult()
                    {
                        Status = (int)HttpStatusCode.OK
                    }));

                }

                return SendMessageAsync(socket, JsonConvert.SerializeObject(new StorageSubscribeMessageResult()
                {
                    Status = (int)HttpStatusCode.BadRequest
                }));
            }
            catch (Exception)
            {
                // Close the socket because the message is not interpreted.
                OnDisconnectedAsync(socket).ConfigureAwait(false);
            }

            return Task.FromResult(0);
        }


        public override Task DisconnectDelegateAsync(WebSocket socket, string clientId)
        {
            RemoveSubscriptionsForClient(clientId);
            return Task.FromResult(0);
        }

        internal void BroadcastChange(string identifier, string key, string data, StorageSubscriptionChangeType changeType)
        {
            // Get all subscriptions
            var clients = GetSubscribedSockets(identifier, key);
            if (clients.Length == 0)
                return;

            // Format the change notification that is send to all subscribed clients
            var message = FormatMessage(identifier, key, data, changeType);
            if (string.IsNullOrEmpty(message))
                return;

            // Retreive all subscribed clients
            var sockets = Manager
                .GetAll()
                .Where(x => clients.Contains(x.Key))
                .Select(x => x.Value)
                .ToList();

            if (sockets.Count == 0)
                return;

            // Publish the message to all connected clients
            sockets.ForEachAsync(Environment.ProcessorCount, async (WebSocket socket) => { await SendMessageAsync(socket, message); }).ConfigureAwait(false);
        }

        private string FormatMessage(string identifier, string key, string data, StorageSubscriptionChangeType changeType)
        {
            return JsonConvert.SerializeObject(new StorageChangeMessage()
            {
                Type = changeType,
                Identifier = identifier,
                Key = key,
                Data = data,
                Timestamp = DateTime.UtcNow
            });
        }

        private string GetSubscriptionKey(StorageSubscribeMessage command) => GetSubscriptionKey(command.Identifier, command.Key);
        private string GetSubscriptionKey(string identifier, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return identifier.ToLowerInvariant();
            }

            return $"{ identifier.ToLowerInvariant() }/{ key.ToLowerInvariant() }";
        }

        private string[] GetSubscribedSockets(string identifier, string key)
        {
            var result = new List<string>();
            var subscriptionKeyExact = GetSubscriptionKey(identifier, key);
            var subscriptionKeyAny = GetSubscriptionKey(identifier, string.Empty);

            lock (_subscriptionsLock)
            {
                foreach (var kvp in _subscriptions)
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
                return false;

            if (string.IsNullOrEmpty(command.Identifier))
                return false;

            var subscriptionKey = GetSubscriptionKey(command);

            lock (_subscriptionsLock)
            {
                if (_subscriptions.ContainsKey(clientId))
                {
                    var subscriptionsForClient = _subscriptions[clientId];
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
                    _subscriptions.Remove(clientId);
            }
        }
    }
}