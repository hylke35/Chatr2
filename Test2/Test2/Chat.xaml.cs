using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Test2.DB;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

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
            SqlDependency.Start(connection.GetConnectionString());
        }

        // Setting up the Join Lobby dialog
        /// <summary>
        /// Setting up the Lobby dialog when clicking on 'Join Lobby' button.
        /// </summary>
        private void LobbyDialogSetup()
        {
            inputBox.AcceptsReturn = false;
            inputBox.Height = 32;
            inputBox.Width = 300;
            inputBox.MaxLength = 6;
            dialogInput.Content = inputBox;
            dialogInput.Title = "Join Lobby";
            dialogInput.IsPrimaryButtonEnabled = true;
            dialogInput.IsSecondaryButtonEnabled = true;
            dialogInput.PrimaryButtonText = "Join";
            dialogInput.SecondaryButtonText = "Cancel";
            dialogInput.PrimaryButtonClick += OnJoinButtonClick_Event;
        }

        // Setting up the Active Lobbies Listbox
        /// <summary>
        ///  Setting up the Active Lobbies Listbox
        /// </summary>
        private async void ActiveLobbiesSetup()
        {
            activeLobbies.Items.Clear();
            IEnumerable<object> getLobbies = await connection.DataReader("SELECT lobbyCode, inProgress FROM dbo.Lobby ORDER BY inProgress ASC", "Lobby");
            List<object> getLobbiesList = getLobbies.ToList();
            foreach (Lobby lobby in getLobbiesList)
            {
                var itemText = lobby.InProgress == 0 ? lobby.LobbyCode : lobby.LobbyCode + " - In Progress";
                activeLobbies.Items.Add(itemText);
            }

            ChangeLobbyTable();
        }

        // DB Listener for checking if Lobbies changed
        /// <summary>
        /// DB Listener for checking if Lobbies changed
        /// </summary>
        public void ChangeLobbyTable()
        {
            using (SqlConnection conn = new SqlConnection(connection.GetConnectionString()))
            {
                conn.Open();
                using (SqlCommand command = new SqlCommand("SELECT lobbyCode, inProgress FROM dbo.Lobby WHERE inProgress = 1 OR inProgress = 0", conn))
                {

                    // Create a dependency and associate it with the SqlCommand.
                    SqlDependency dependency = new SqlDependency(command);
                    // Maintain the reference in a class member.

                    // Subscribe to the SqlDependency event.
                    dependency.OnChange += new OnChangeEventHandler(OnChangeLobbyTable);

                    //Execute the command.
                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                    }
                }
                conn.Close();
            }
        }

        // Event, that gets triggerd when lobby properties change
        /// <summary>
        ///  Event, that gets triggerd when lobby properties change
        /// </summary>
        private async void OnChangeLobbyTable(object sender, SqlNotificationEventArgs e)
        {
            // If data updated, inserted or delted
            if (e.Type == SqlNotificationType.Change && (e.Info == SqlNotificationInfo.Update || e.Info == SqlNotificationInfo.Insert || e.Info == SqlNotificationInfo.Delete))
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => {
                    activeLobbies.Items.Clear();
                    // Get Lobbies from DB
                    IEnumerable<object> getLobbies = await connection.DataReader("SELECT lobbyCode, inProgress FROM dbo.Lobby ORDER BY inProgress ASC", "Lobby");
                    List<object> getLobbiesList = getLobbies.ToList();
                    foreach (Lobby lobby in getLobbiesList)
                    {
                        var itemText = lobby.InProgress == 0 ? lobby.LobbyCode : lobby.LobbyCode + " - In Progress";
                        activeLobbies.Items.Add(itemText);
                    }
                }).AsTask();
            }
            SqlDependency dependency = sender as SqlDependency;
            ChangeLobbyTable();
        }

        // Event that gets triggered when Join Lobby button is clicked
        /// <summary>
        /// Event that gets triggered when Join Lobby button is clicked.  
        /// </summary>
        private async void OnJoinButtonClick_Event(object sender, ContentDialogButtonClickEventArgs e)
        {
            // Get UserLobby records from current User
            IDictionary<string, object> userLobbyCheckDictionary = new Dictionary<string, object>();
            userLobbyCheckDictionary.Add("@userID", userID);
            IEnumerable<object> userLobbyCheck = await connection.DataReader("SELECT * FROM dbo.User_Lobby WHERE userID = @userID", "User_Lobby", userLobbyCheckDictionary);

            // Check if User has record in User_Lobby table, if not continue
            if (userLobbyCheck.ToList().Count == 0)
            {
                var input = inputBox.Text;
                
                // Get Lobby with specified lobbyCode from user input
                IDictionary<string, object> selectLobbyDictionary = new Dictionary<string, object>();
                selectLobbyDictionary.Add("@code", input);
                IEnumerable<object> selectLobby = await connection.DataReader("SELECT * FROM dbo.Lobby WHERE lobbyCode = @code", "Lobby", selectLobbyDictionary);
                List<object> selectLobbyList = selectLobby.ToList();

                // Check if specified Lobby exists
                if (selectLobbyList.Count != 0)
                {
                    Lobby lobby = (Lobby) selectLobbyList.First();

                    // Check if Lobby is already in progress, if not continue
                    if (lobby.InProgress == 0)
                    {
                        // Insert User into the Lobby
                        IDictionary<string, object> insertUserLobbyDictionary = new Dictionary<string, object>();
                        insertUserLobbyDictionary.Add("@code", input);
                        insertUserLobbyDictionary.Add("@userID", userID);
                        connection.RunQueryAsync("INSERT INTO dbo.User_Lobby(userID, lobbyCode, isReady, isLoaded) VALUES (@userID, @code, 0, 0)", insertUserLobbyDictionary);

                        // Create new Window and pass lobbyCode and userID
                        Redirect(input, userID);
                    }
                    else
                    {
                        // Lobby is already in progress
                        var dialog = new MessageDialog("Video Party is already in progress!", "Error");
                        await dialog.ShowAsync();
                    }

                }
                else
                {
                    // The lobby does not exist.
                    var dialog = new MessageDialog("Lobby does not exist!", "Error");
                    await dialog.ShowAsync();
                }
            }
            else
            {
                // User already exists in User_Lobby table
                var dialog = new MessageDialog("You already joined a Video Party!", "Error");
                await dialog.ShowAsync();
            }


        }

        // Creates a new window and passes parameters
        /// <summary>
        /// Creates a new window and passes parameters  
        /// </summary>
        /// <param name="lobbyCode">The specifired Lobby code to join the lobby</param>
        /// <param name="userID">The current userID</param>
        private async void Redirect(string lobbyCode, int userID)
        {
            var parameters = new Params();
            parameters.LobbyCode = lobbyCode;
            parameters.UserId = userID;

            // Creates new ApplicationView
            var currentAV = ApplicationView.GetForCurrentView();
            var newAV = CoreApplication.CreateNewView();
            
            await newAV.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                async () =>
                {
                    var newWindow = Window.Current;
                    var newAppView = ApplicationView.GetForCurrentView();
                    newAppView.Title = "Video Lobby - " + lobbyCode;
                    var frame = new Frame();
                    // Navigates to new page
                    frame.Navigate(typeof(VideoLobby), parameters);
                    newWindow.Content = frame;
                    newWindow.Activate();
                    await ApplicationViewSwitcher.TryShowAsStandaloneAsync(
                        newAppView.Id,
                        ViewSizePreference.UseMinimum,
                        currentAV.Id,
                        ViewSizePreference.UseMinimum);
                }).AsTask();

            SqlDependency.Stop(connection.GetConnectionString());
        }

        // Event that gets triggered when navigated to this page
        /// <summary>
        /// Event that gets triggered when navigated to this page 
        /// </summary>
        /// <param name="e">Event variable</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Maps the passed parameters to this page
            var parameters = (LoginParams)e.Parameter;
            userID = parameters.UserID;
            username = parameters.UserName;
            LobbyDialogSetup();
            ActiveLobbiesSetup();
        }

        // Event that gets triggered clicking on Create Lobby button
        /// <summary>
        /// Event that gets triggered clicking on Create Lobby button which opens a new Lobby window
        /// </summary>
        private async void createLobby_Click(object sender, RoutedEventArgs e)
        {
            // Get UserLobby records from current User
            IDictionary<string, object> userLobbyCheckDictionary = new Dictionary<string, object>();
            userLobbyCheckDictionary.Add("@userID", userID);
            IEnumerable<object> userLobbyCheck = await connection.DataReader("SELECT * FROM dbo.User_Lobby WHERE userID = @userID", "User_Lobby", userLobbyCheckDictionary);

            // Check if User has record in User_Lobby table, if not continue
            if (userLobbyCheck.ToList().Count == 0)
            {
                var code = App.RandomString(6);

                // Inserts user into Lobby
                IDictionary<string, object> insertUserLobbyDictionary = new Dictionary<string, object>();
                insertUserLobbyDictionary.Add("@code", code);
                insertUserLobbyDictionary.Add("@userID", userID);
                connection.RunQueryAsync("INSERT INTO dbo.Lobby(lobbyCode, inProgress) VALUES (@code, 0);INSERT INTO dbo.User_Lobby(userID, lobbyCode, isReady, isLoaded) VALUES (@userID, @code, 0, 0)", insertUserLobbyDictionary);

                // Create new Window and pass lobbyCode and userID
                Redirect(code, userID);
            }
            else
            {
                // User already joined a Lobby
                var dialog = new MessageDialog("You already joined a Video Party!", "Error");
                await dialog.ShowAsync();
            }

        }

        // Event that gets triggered clicking on Join Lobby button
        /// <summary>
        /// Event that gets triggered clicking on Join Lobby button which opens a new Lobby window
        /// </summary>
        private async void joinLobby_Click(object sender, RoutedEventArgs e)
        {
            // Pop up
            await dialogInput.ShowAsync();
        }

        private void send_Click(object sender, RoutedEventArgs e)
        {
            (Application.Current as App).Broadcast(new ChatMessage { Username = username, Message = text.Text });
            text.Text = "";
        }
    }
}
