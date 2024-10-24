using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        using (ClientWebSocket webSocket = new ClientWebSocket())
        {
            Uri serverUri = new Uri("ws://36.0.0.29:5000/ws/");


                await webSocket.ConnectAsync(serverUri, CancellationToken.None);
            Console.WriteLine("Connected to WebSocket server.");
            string response;
            while (true)
            {
                // Handle server prompts for User ID and Password
                
                string userId = Console.ReadLine();
                await SendMessageAsync(webSocket, userId);

                
                string password = Console.ReadLine();
                await SendMessageAsync(webSocket, password);

                // Receive authentication result
                response = await ReceiveMessageAsync(webSocket);
                Console.WriteLine(response);
                if (response.Contains("Authentication successful"))
                {
                    break;
                }
                
            }
            
            Console.WriteLine("Authentication successful! You can now send messages.");
            

            // Start receiving messages from the server
            Task receiveTask = ReceiveMessagesAsync(webSocket);

            // Loop to send custom messages to the server
            while (webSocket.State == WebSocketState.Open)
            {
                
                string message = Console.ReadLine();

                if (message.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                if (string.IsNullOrEmpty(message))
                {
                    Console.WriteLine("Message cannot be empty. Try again.");
                    continue;
                }

                await SendMessageAsync(webSocket, message);
            }

            // Close the WebSocket connection
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed connection", CancellationToken.None);
            await receiveTask;
        }
    }

    private static async Task SendMessageAsync(ClientWebSocket webSocket, string message)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private static async Task<string> ReceiveMessageAsync(ClientWebSocket webSocket)
    {
        byte[] buffer = new byte[1024];
        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        return Encoding.UTF8.GetString(buffer, 0, result.Count);
    }

    private static async Task ReceiveMessagesAsync(ClientWebSocket webSocket)
    {
        byte[] buffer = new byte[1024 * 4];
        while (webSocket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                Console.WriteLine("Closed WebSocket connection.");
            }
            else
            {
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine(message);
            }
        }
    }
}
