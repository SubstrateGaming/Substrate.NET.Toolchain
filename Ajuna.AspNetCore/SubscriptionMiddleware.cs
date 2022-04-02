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
        private SubscriptionHandler _webSocketHandler { get; set; }

        public SubscriptionMiddleware(RequestDelegate next, SubscriptionHandler webSocketHandler)
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

            await ReceiveAsync(socket, (result, buffer) =>
            {
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    _webSocketHandler.OnReceivedAsync(socket, result, buffer).ConfigureAwait(false);
                }

                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    _webSocketHandler.OnDisconnectedAsync(socket).ConfigureAwait(false);
                }

            });
        }

        private async Task ReceiveAsync(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer), cancellationToken: CancellationToken.None);
                handleMessage(result, buffer);
            }
        }
    }
}
