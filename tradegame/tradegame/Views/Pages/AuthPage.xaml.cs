using System.Windows;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using tradegame.Views.Pages;
using System;
using System.Windows.Controls;
using System.Threading;
using tradegame.Services;
using tradegame.Utilities;

namespace tradegame.Views.Pages
{
    public partial class AuthPage : Page
    {
        private ClientWebSocket _clientWebSocket;
        private NavigationService _navigationService;
       

        public AuthPage(NavigationService navigationService)
        {
            InitializeComponent();
            _clientWebSocket = new ClientWebSocket();
            _navigationService = navigationService;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;
            string response = await WebSocketUtil.Authenticate(username, password);

            _clientWebSocket = WebSocketUtil._clientWebSocket;
            
            MessageBox.Show(response);

            if (response == "Authentication successful! You can now send messages.")
            {
                UserDetaiils.UserName = username;
                _navigationService.NavigateTo(new ChatPage(_clientWebSocket,_navigationService));
            }
        }

        private async Task SendMessageAsync(string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await _clientWebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task<string> ReceiveMessageAsync()
        {
            byte[] buffer = new byte[1024];
            var result = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            return Encoding.UTF8.GetString(buffer, 0, result.Count);
        }
    }
}
