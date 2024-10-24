using System;
using System.Collections.Concurrent;
using MySql.Data;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using serverportal;
using System.Collections.Generic;
using Newtonsoft.Json;
using serverportal.Models;

class Program
{
    private static ConcurrentDictionary<string, WebSocket> _connectedClients = new ConcurrentDictionary<string, WebSocket>();
    
    private static List<UserDetails> Details = new List<UserDetails>();
    private static int PlayerCount = 0;
    private static ConcurrentDictionary<string, WebSocket> _GameClients = new ConcurrentDictionary<string, WebSocket>();

    static async Task Main(string[] args)
    {
        LoadData();
        HttpListener httpListener = new HttpListener();
        httpListener.Prefixes.Add("http://*:5000/ws/");
        httpListener.Start();
        Console.WriteLine("WebSocket server started at ws://localhost:5000/ws/");

        while (true)
        {
            HttpListenerContext context = await httpListener.GetContextAsync();
            if (context.Request.IsWebSocketRequest)
            {
                HttpListenerWebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
                WebSocket webSocket = wsContext.WebSocket;

                Task handleClientTask = HandleClientAsync(webSocket);
                _ = handleClientTask.ContinueWith(t => webSocket.Dispose()); // Ensure the WebSocket is disposed after handling
            }
            else
            {
                context.Response.StatusCode = 400;
                context.Response.Close();
            }
        }
    }
    private static void LoadData()
    {
        string connectionString = @"Server=localhost;
                                           Port=3306;
                                           Database=businessgame;
                                           User Id=root;
                                           Password=BTharun@159;";



        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                Console.WriteLine("Connection successful!");

                // Perform database operations here
                string query = "SELECT * FROM userdetails";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int id = (int)reader["UserId"];
                    string username = (string)reader["username"];
                    string password = (string)reader["password"];
                    Details.Add(new UserDetails(id, username, password));
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Connection failed: " + ex.Message);
            }
        }
    }
    private static bool Authenticate(string username, string password)
    {
        foreach (var detail in Details)
        {
            if (detail.userName == username && detail.passWord == password)
            {
                return true;
            }
        }
        return false;
    }

    private static async Task HandleClientAsync(WebSocket webSocket)
    {
        string username = null;

        while (webSocket.State == WebSocketState.Open)
        {
            // Ask for User ID
           
            username = await ReceiveMessageAsync(webSocket);

            // Ask for Password
           
            string password = await ReceiveMessageAsync(webSocket);

            // Authenticate
            if (Authenticate(username,password))
            {
                await SendMessageAsync(webSocket, "Authentication successful! You can now send messages.");
                _connectedClients.TryAdd(username, webSocket);
                await CommunicateAsync(webSocket, username);
                break;
            }
            else
            {
                await SendMessageAsync(webSocket, "Invalid credentials. Please try again.");
            }
        }
    }

    private static async Task CommunicateAsync(WebSocket webSocket, string userId)
    {
        string connectionString = @"Server=localhost;
                                           Port=3306;
                                           Database=businessgame;
                                           User Id=root;
                                           Password=BTharun@159;";
        byte[] buffer = new byte[1024 * 4];
        while (webSocket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                break;
            }

            string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            Message msg = new Message();
            if (CanDeserialize<CommunicationCode>(message))
            {
                
                }
            else if (CanDeserialize<Message>(message))
            {
                msg = JsonConvert.DeserializeObject<Message>(message);
                message = msg.message;
                
            }
            else
            {
                if (message == "waiting")
                {
                    _GameClients.TryAdd(userId, webSocket);
                    if (PlayerCount == 0)
                    {
                        await SendMessageAsync(webSocket, "yeschance");
                    }
                    else
                    {
                        await SendMessageAsync(webSocket, "nochance");

                    }
                    PlayerCount++;
                    
                }
                else
                {
                    msg = new Message("Chat", message);
                }

                }
                Console.WriteLine($"Received from {userId}: {message}");

            if (message != "waiting")
            {
                foreach (var client in _connectedClients)
                {
                    if (client.Key != userId)
                    {
                        string type = msg.MessageType;

                        if (type == "Chat")
                        {
                            msg.message = $"{userId}: {message}";

                            string jsonstring = JsonConvert.SerializeObject(msg);

                            await SendMessageAsync(client.Value, jsonstring);

                        }
                        else if (type == "Game")
                        {
                            string jsonstring = JsonConvert.SerializeObject(msg);

                            await SendMessageAsync(client.Value, jsonstring);

                        }

                    }
                }
            }
        }

        _connectedClients.TryRemove(userId, out _);
        if (_GameClients.ContainsKey(userId))
        {
            PlayerCount--;
        }
        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
    }

    private static async Task SendMessageAsync(WebSocket webSocket, string message)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private static async Task<string> ReceiveMessageAsync(WebSocket webSocket)
    {
        byte[] buffer = new byte[1024];
        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        return Encoding.UTF8.GetString(buffer, 0, result.Count);
    }
    public static bool CanDeserialize<T>(string json)
    {
        try
        {
            JsonConvert.DeserializeObject<T>(json);
            return true; // Deserialization was successful
        }
        catch (JsonException)
        {
            return false; // Deserialization failed
        }
    }

}
