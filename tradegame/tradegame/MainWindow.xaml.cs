using System.Windows;
using tradegame.Views.Pages;
using tradegame.Services;
using tradegame.Utilities;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace tradegame
{
    public partial class MainWindow : Window
    {
        private NavigationService _navigationService;
        private ClientWebSocket _clientWebSocket;
        public MainWindow()
        {
            InitializeComponent();
            _navigationService = new NavigationService(MainFrame);
            _navigationService.NavigateTo(new AuthPage(_navigationService));
            StartReceivingMessagesInBackground();
        }

        private async void StartReceivingMessagesInBackground()
        {
            
            while (true)
            {
                _clientWebSocket = WebSocketUtil._clientWebSocket;

                if (_clientWebSocket != null && _clientWebSocket.State == WebSocketState.Open)
                {
                    await ReceiveMessages();
                }

                await Task.Delay(500); // Delay to prevent excessive CPU usage
            }
        }

        public async Task ReceiveMessages()
        {
            await WebSocketUtil.ReceiveMessages();
        }

    }
}
