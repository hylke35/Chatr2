using EasyEncryption;
using System;
using System.Collections.Generic;
using System.Linq;
using Test2.DB;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Test2
{
    /// <summary>
    /// This page is used to register accounts and login to the chat application.
    /// </summary>
    public sealed partial class Login : Page
    {
        DBConnect connection = new DBConnect();
        public Login()
        {
            this.InitializeComponent();
        }

        private async void loginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = userBox.Text;
            string password = passBox.Password.ToString();

            //Hashing password entered in password textbox
            string hashedPassword = HashPassword(password);

            IDictionary<string, object> passwordDict = new Dictionary<string, object>();
            passwordDict.Add("@username", username);
            passwordDict.Add("@password", hashedPassword);
            IEnumerable<object> passwordEnumerable = await connection.DataReader("SELECT * FROM dbo.Users WHERE username = @username AND password = @password", "Users", passwordDict);

            List<object> passwordList = passwordEnumerable.ToList();

            string databasePassword = null;
            int userID = 0;

            foreach (User u in passwordList)
            {
                databasePassword = u.Password;
                userID = u.UserId;
            }

            //Compare hashed password saved in database to password textbox password converted to hash
            if (PasswordsMatch(password, databasePassword))
            {
                statusBox.Text = "Success, you will be redirected to the chat";

                //Setting parameters prior to entering "Chat" page and then navigating to "Chat"
                var parameters = new LoginParams();
                parameters.UserName = username;
                parameters.UserID = userID;
                Frame.Navigate(typeof(Chat), parameters);
            }
            else
            {
                statusBox.Text = "Failure, the username and password don't match!";
            }
        }

        private async void registerButton_Click(object sender, RoutedEventArgs e)
        {
            string username = userBox.Text;
            string password = passBox.Password.ToString();
            string hashedPassword = HashPassword(password);

            IDictionary<string, object> userNameDict = new Dictionary<string, object>();
            userNameDict.Add("@username", username);
            IEnumerable<object> userNameEnumerable = await connection.DataReader("SELECT * FROM dbo.Users WHERE username = @username", "Users", userNameDict);

            List<object> userNameList = userNameEnumerable.ToList();

            string checkUsername = null;

            foreach (User u in userNameList)
            {
                checkUsername = u.Username;
            }

            //Compare username in username textbox to already exisiting users in database
            if (checkUsername != username)
            {
                IDictionary<string, object> insertUserDict = new Dictionary<string, object>();
                insertUserDict.Add("@username", username);
                insertUserDict.Add("@password", hashedPassword);
                connection.RunQueryAsync("INSERT INTO dbo.Users (username, password) VALUES (@username, @password)", insertUserDict);
                statusBox.Text = "Success, account with this username has been created!";

                IDictionary<string, object> selectUserDict = new Dictionary<string, object>();
                selectUserDict.Add("@username", username);
                IEnumerable<object> selectUserEnumerable = await connection.DataReader("SELECT * FROM dbo.Users WHERE username = @username", "Users", selectUserDict);

                List<object> list2 = selectUserEnumerable.ToList();

                int userID = 0;

                foreach (User u in list2)
                {
                    userID = u.UserId;
                }

                //Setting parameters prior to entering "Chat" page and then navigating to "Chat"
                var parameters = new LoginParams();
                parameters.UserName = username;
                parameters.UserID = userID;
                Frame.Navigate(typeof(Chat), parameters);
            }
            else
            {
                statusBox.Text = "Failure, account with this username already exists!";
            }

        }

        //Convert input into SHA256 Hash
        private static string HashPassword(string input)
        {
            return SHA.ComputeSHA256Hash(input);
        }

        //Compare 2 inputs
        private static bool PasswordsMatch(string userInput, string passwordString)
        {
            string hashedInput = HashPassword(userInput);
            bool doPasswordsMatch = string.Equals(hashedInput, passwordString);
            return doPasswordsMatch;
        }
    }

    public class LoginParams
    {
        public LoginParams() { }
        public string UserName { get; set; }
        public int UserID { get; set; }
    }
}
