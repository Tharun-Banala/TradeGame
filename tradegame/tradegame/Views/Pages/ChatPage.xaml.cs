
using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Net.WebSockets;
using System.Windows.Threading;
using System.Threading;
using tradegame.Services;
using tradegame.Models;
using tradegame.Utilities;
using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;

namespace tradegame.Views.Pages
{
    public partial class ChatPage : Page
    {
        private ClientWebSocket _clientWebSocket;
        private NavigationService _navigationService;
        public ChatPage(ClientWebSocket clientWebSocket, NavigationService navigationService)
        {
            InitializeComponent();
            _clientWebSocket = clientWebSocket;
            _navigationService = navigationService;
            WebSocketUtil.OnMessageReceived += OnMessageReceived;



        }

        private void OnMessageReceived(string message)
        {
            if (JsonUtil.CanDeserialize<Message>(message))
            {
                Message msg = JsonConvert.DeserializeObject<Message>(message);
                if (msg.MessageType == "Chat")
                {
                    Dispatcher.Invoke(() => MessagesListBox.Items.Add(msg.message));
                }
            }
            return;
            
        }
            private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string message = MessageInputTextBox.Text;
            await WebSocketUtil.SendChatMessageAsync(message);
            MessageInputTextBox.Clear();
            Dispatcher.Invoke(() => MessagesListBox.Items.Add($"you: {message}"));
        }

        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {

            _navigationService.NavigateTo(new GamePage(_clientWebSocket));
        }

    }
}

