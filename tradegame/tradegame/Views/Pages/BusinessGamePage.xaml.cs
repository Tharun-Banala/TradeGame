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
using tradegame.Models;

namespace tradegame.Views.Pages
{
    /// <summary>
    /// Interaction logic for BusinessGamePage.xaml
    /// </summary>
    /// 

    public partial class BusinessGamePage : Page
    {
        private ClientWebSocket _clientWebSocket;
        private NavigationService _navigationService;
        public BusinessGamePage(NavigationService navigation)
        {
            InitializeComponent();
            _clientWebSocket = new ClientWebSocket();
            _navigationService = navigation;
            
        }
        
    }
}
