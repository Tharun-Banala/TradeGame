using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using tradegame.Models;
using System.Windows;
namespace tradegame.Utilities
{
    public static class WebSocketUtil
    {
        public static ClientWebSocket _clientWebSocket { get; set; }
        public static event Action<string> OnMessageReceived;

        public static async Task<string> Authenticate(string username, string password)
        {
            _clientWebSocket = new ClientWebSocket();
            await _clientWebSocket.ConnectAsync(new Uri("ws://36.0.0.29:5000/ws/"), CancellationToken.None);

            // Send authentication details
            await SendMessageAsync(username);
            await SendMessageAsync(password);

            // Wait for authentication response
            string response = await ReceiveMessageAsync();
            return response;
        }
        public static async Task SendMessageAsync(string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await _clientWebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public static async Task<string> ReceiveMessageAsync()
        {
            byte[] buffer = new byte[1024];
            var result = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            return Encoding.UTF8.GetString(buffer, 0, result.Count);
        }

        public static async Task ReceiveMessages()
        {
            byte[] buffer = new byte[1024];

            while (_clientWebSocket.State == WebSocketState.Open)
            {
                var result = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);


                    OnMessageReceived?.Invoke(message);
                }
            }
        }
        public static async Task SendChatMessageAsync(string message)
        {
            
            if (!string.IsNullOrEmpty(message))
            {
                Message msg = new Message("Chat", message);
                message= JsonConvert.SerializeObject(msg);
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                await _clientWebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                
            }
        }

        public static async Task SendGameMessageAsync(List<Point> points)
        {

            if (points!=null && points.Any())
            {
                Message msg = new Message("Game", "");
                msg.points = points;
                string message = JsonConvert.SerializeObject(msg);
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                await _clientWebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);

            }
        }
        public static async Task SendWaitingMessageAsync(string message)
        {

            if (!string.IsNullOrEmpty(message))
            {
               
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                await _clientWebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);

            }
        }

        
    }
}
