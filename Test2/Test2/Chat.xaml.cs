using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.ViewManagement;
using Windows.ApplicationModel.Core;
using Windows.UI.Popups;
using System.Threading.Tasks;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Test2
{
    public sealed partial class Chat : Page
    {
        int userID;
        string username;
        DBConnect connection = new DBConnect();

        ContentDialog dialogInput = new ContentDialog();
        TextBox inputBox = new TextBox();


        public Chat()
        {
            this.InitializeComponent();
            this.DataContext = (Application.Current as App).ChatVM;
            
        }

        private void Setup()
        {
            inputBox.AcceptsReturn = false;
            inputBox.Height = 32;
            inputBox.Width = 300;
            inputBox.MaxLength = 6;
/*            inputBox.TextChanging += TextChangingHandler;*/
            dialogInput.Content = inputBox;
  
            dialogInput.Title = "Join Lobby";
            dialogInput.IsPrimaryButtonEnabled = true;
            dialogInput.IsSecondaryButtonEnabled = true;
            dialogInput.PrimaryButtonText = "Join";
            dialogInput.SecondaryButtonText = "Cancel";
            dialogInput.PrimaryButtonClick += join;
        }
        private async void join(object sender, ContentDialogButtonClickEventArgs e)
        {
            var input = inputBox.Text;

            IDictionary<string, object> p = new Dictionary<string, object>();
            p.Add("@code", input);

            IEnumerable<object> list2 = await connection.DataReader("SELECT * FROM dbo.Lobby WHERE lobbyCode = @code", "Lobby", p);

            List<object> test2 = list2.ToList();
            
            if (test2.Count != 0)
            {
                Lobby lobby = (Lobby) test2.First();
                if (lobby.InProgress == 0)
                {
                    IDictionary<string, object> p2 = new Dictionary<string, object>();
                    p2.Add("@code", input);
                    p2.Add("@userID", this.userID);

                    connection.runQueryAsync("INSERT INTO dbo.User_Lobby(userID, lobbyCode, isReady, isLoaded) VALUES (@userID, @code, 0, 0)", p2);

                    var parameters = new Params();
                    parameters.LobbyCode = input;
                    parameters.UserId = userID;
                    var currentAV = ApplicationView.GetForCurrentView();
                    var newAV = CoreApplication.CreateNewView();
                    await newAV.Dispatcher.RunAsync(
                        CoreDispatcherPriority.Normal,
                        async () =>
                        {
                            var newWindow = Window.Current;
                            var newAppView = ApplicationView.GetForCurrentView();
                            newAppView.Title = "Video Lobby - " + input;

                            var frame = new Frame();
                            
                            frame.Navigate(typeof(VideoLobby), parameters);
                            newWindow.Content = frame;
                            newWindow.Activate();
                            
                            
                            await ApplicationViewSwitcher.TryShowAsStandaloneAsync(
                                newAppView.Id,
                                ViewSizePreference.UseMinimum,
                                currentAV.Id,
                                ViewSizePreference.UseMinimum);
                        });

                } else
                {
                    var dialog = new MessageDialog("Video Party is already in progress!", "Error");
                    await dialog.ShowAsync();
                }

            } else
            {
                var dialog = new MessageDialog("Lobby does not exist!", "Error");
                await dialog.ShowAsync();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var parameters = (LoginParams)e.Parameter;
            userID = parameters.UserID;
            username = parameters.UserName;

            Setup();
        }

        private void send_Click(object sender, RoutedEventArgs e)
        {
            (Application.Current as App).Broadcast(new ChatMessage { Username = username , Message = text.Text });
            text.Text = "";
        }

        private void createLobby_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private async void joinLobby_Click(object sender, RoutedEventArgs e)
        {
            await dialogInput.ShowAsync();
        }
    }
}
