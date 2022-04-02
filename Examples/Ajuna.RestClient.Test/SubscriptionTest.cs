using Ajuna.Restclient.Subscription.Model;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ajuna.RestClient.Test
{
    public class SubscriptionTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task BasicSubscribeTestAsync()
        {
            var client = new ClientWebSocket();
            await client.ConnectAsync(new Uri("ws://localhost:5000/ws"), CancellationToken.None);

            var message = new StorageSubscribeMessage()
            {
                Identifier = "Assets.Asset"
            };

            // Subscribe to Lottery
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            await client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);

            // Get the confirmation
            var subscriptionResponse = await ReceiveMessageAsync(client);
            Assert.IsFalse(string.IsNullOrEmpty(subscriptionResponse));

            var subscriptionResponseResult = JsonConvert.DeserializeObject<StorageSubscribeMessageResult>(subscriptionResponse);
            Assert.IsNotNull(subscriptionResponseResult);
            Assert.IsTrue(subscriptionResponseResult.Status == (int)HttpStatusCode.OK);

            // Now read a couple of subscriptions.
            // Make sure to create 1 new asset in explorer while running this test.
            await ReceiveMultipleSubscriptionsAsync(client);
        }

        private async Task<string> ReceiveMessageAsync(ClientWebSocket client)
        {
            // The buffer here should be enough for all cases since we expect
            // a short message indicating the command status.
            var buffer = new byte[1024 * 4];
            var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Text && result.EndOfMessage)
                return Encoding.UTF8.GetString(buffer, 0, result.Count);

            await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            return null;
        }

        private async Task ReceiveMultipleSubscriptionsAsync(ClientWebSocket client)
        {
            // The buffer here should be enough for tests.
            // In real scenarios we must read until the end of a message because transmitted data
            // may be larger than the given buffer size.
            var buffer = new byte[1024 * 4];

            int receivedSubscriptions = 0;

            while (true)
            {
                var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text && result.EndOfMessage)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Assert.IsNotNull(message);
                    Assert.IsNotEmpty(message);

                    var subscriptionChangeMessage = JsonConvert.DeserializeObject<StorageChangeMessage>(message);
                    Assert.IsNotNull(subscriptionChangeMessage);

                    receivedSubscriptions++;

                    if (receivedSubscriptions == 1)
                    {
                        break;
                    }

                }

                else if (result.MessageType == WebSocketMessageType.Close)
                    break;
            }

            await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            Assert.IsTrue(receivedSubscriptions == 5);
        }
    }
}