using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AuthorizationUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private string connectionString = @"Data Source=DESKTOP-8NRQUKJ\MSSQL;Initial Catalog=ChatrDB;Integrated Security=True";
        private Boolean succesfullyRegistered;
        private Boolean succesfullyLoggedIn;



        public MainPage()
        {
            this.InitializeComponent();

        }



        private async Task InsertUserAsync() {
            try
            {
                var query = "Insert INTO Users(username,password) Values('" + userName.Text.Trim()  + "', '" + passWord.Password + "')";

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var users = new Users
                                {
                                    userID = reader.GetInt32(0)
                                };

                                if (!await reader.IsDBNullAsync(1))
                                    users.username = reader.GetString(1);

                                if (!await reader.IsDBNullAsync(2))
                                    users.email = reader.GetString(2);

                                if (!await reader.IsDBNullAsync(3))
                                    users.password = reader.GetString(3);

                                if (!await reader.IsDBNullAsync(4))
                                    users.profileName = reader.GetString(4);

                                if (!await reader.IsDBNullAsync(5))
                                    users.status = reader.GetString(5);

                                if (!await reader.IsDBNullAsync(6))
                                    users.age = reader.GetInt32(6);

                                if (!await reader.IsDBNullAsync(7))
                                    users.profilePicture = reader.GetString(7);
                            }
                        }
                    }
                }
                succesfullyRegistered = true;
            }
            catch (Exception) 
            {
                throw;
            }
        }


        private async Task GetUserAsync()
        {
            try
            {
                var query = "Select username,password From Users Where username = '" + userName.Text.Trim() + "' And password = '" + passWord.Password + "'";

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                SqlDataAdapter sda = new SqlDataAdapter(query, connectionString);
                                DataTable dtb1 = new DataTable();
                                sda.Fill(dtb1);
                                if (dtb1.Rows.Count == 1)
                                {
                                    succesfullyLoggedIn = true;
                                }
                                else
                                {
                                    succesfullyLoggedIn = false;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await GetUserAsync();
                if (succesfullyLoggedIn == true)
                {
                    messageBox.Text = "Logged in!";
                }
                else
                {
                    messageBox.Text = "Logged off!";
                }
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                await InsertUserAsync();
                if (succesfullyRegistered == true)
                {
                    messageBox.Text = "Successfully Registered!";
                }
                else
                {
                    messageBox.Text = "Unsuccessfully Registered!";
                }
            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}
