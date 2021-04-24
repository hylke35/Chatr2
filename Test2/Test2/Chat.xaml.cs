using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Test2
{
    public sealed partial class Chat : Page
    {
        int userID;
        string username;
        public Chat()
        {
            this.InitializeComponent();
            this.DataContext = (Application.Current as App).ChatVM;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var parameters = (LoginParams)e.Parameter;
            userID = parameters.UserID;
            username = parameters.UserName;
        }

        private void send_Click(object sender, RoutedEventArgs e)
        {
            (Application.Current as App).Broadcast(new ChatMessage { Username = username , Message = text.Text });
            text.Text = "";
        }

        
    }
}
