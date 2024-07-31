using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class WebSocketServerService
{
    private readonly HttpListener _listener;
    private readonly ConcurrentDictionary<WebSocket, Task> _clients;

    public WebSocketServerService(string uriPrefix)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add(uriPrefix);
        _clients = new ConcurrentDictionary<WebSocket, Task>();
    }

    public void Start()
    {
        _listener.Start();
        Console.WriteLine("WebSocket server started...");
        Task.Run(AcceptLoop);
    }

    public void Stop()
    {
        foreach (var client in _clients.Keys)
        {
            client.Abort();
        }
        _listener.Stop();
        Console.WriteLine("WebSocket server stopped.");
    }

    private async Task AcceptLoop()
    {
        while (_listener.IsListening)
        {
            var context = await _listener.GetContextAsync();
            if (context.Request.IsWebSocketRequest)
            {
                var wsContext = await context.AcceptWebSocketAsync(null);
                var webSocket = wsContext.WebSocket;
                _clients[webSocket] = Task.Run(() => HandleClient(webSocket));
            }
            else
            {
                context.Response.StatusCode = 400;
                context.Response.Close();
            }
        }
    }

    private async Task HandleClient(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        var segment = new ArraySegment<byte>(buffer);

        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(segment, CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
            else if (result.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"Received: {message}");
                await BroadcastMessageAsync(message);
            }
        }

        _clients.TryRemove(webSocket, out _);
    }

    public async Task BroadcastMessageAsync(string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        var segment = new ArraySegment<byte>(buffer);

        foreach (var client in _clients.Keys)
        {
            if (client.State == WebSocketState.Open)
            {
                await client.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}

