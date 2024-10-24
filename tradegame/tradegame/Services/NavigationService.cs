using System;
using System.Windows;
using System.Windows.Controls;

namespace tradegame.Services
{
    public class NavigationService
    {
        private Frame _mainFrame;

        public NavigationService(Frame mainFrame)
        {
            _mainFrame = mainFrame;
        }

        public void NavigateTo(Page page)
        {
            _mainFrame.Navigate(page);
        }

        public void GoBack()
        {
            if (_mainFrame.CanGoBack)
            {
                _mainFrame.GoBack();
            }
        }
    }
}
