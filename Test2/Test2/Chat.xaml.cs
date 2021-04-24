using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Test2
{
    public sealed partial class Chat : Page
    {
        public Chat()
        {
            this.InitializeComponent();
            this.DataContext = (Application.Current as App).ChatVM;
        }

        private void send_Click(object sender, RoutedEventArgs e)
        {
            (Application.Current as App).Broadcast(new ChatMessage { Username = "user1" , Message = text.Text });
            text.Text = "";
        }
    }
}
