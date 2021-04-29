using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Test2
{
    public class DBConnect
    {
        //string asyncConnectionString = new SqlConnectionStringBuilder(@"Data Source=20.52.146.90,1433;Initial Catalog=ChatrTestDB;User ID=hemran;Password=TestPassword!;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False").ToString();
        string asyncConnectionString = new SqlConnectionStringBuilder(@"Data Source=51.116.224.130,1433;Initial Catalog=ChatrTestDB;User ID=chatr;Password=TestPassword1!;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False").ToString();
        public string getConnectionString()
        {
            return asyncConnectionString;
        }



        public async void runQueryAsync(string query, IDictionary<string, object> addwithvalue = null)
        {
            using (SqlConnection connection = new SqlConnection(asyncConnectionString))
            {
                await connection.OpenAsync();
                SqlCommand command = connection.CreateCommand();

                try
                {
                    command.CommandText = query;

                    if (addwithvalue != null)
                    {
                        foreach (KeyValuePair<string, object> kvp in addwithvalue)
                        {
                            command.Parameters.AddWithValue(kvp.Key, kvp.Value);
                        }
                    }

                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);

                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Something went wrong:" + ex.Message);
                }
            }
        }





        public async Task<IEnumerable<object>> DataReader(string query, string table, IDictionary<string, object> addwithvalue = null)
        {
            var results = new List<object>();
            using (SqlConnection connection = new SqlConnection(asyncConnectionString))
            {
                await connection.OpenAsync();
                SqlCommand command = connection.CreateCommand();

                try
                {
                    command.CommandText = query;
                    if (addwithvalue != null)
                    {
                        foreach (KeyValuePair<string, object> kvp in addwithvalue)
                        {
                            command.Parameters.AddWithValue(kvp.Key, kvp.Value);
                        }
                    }
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            switch (table)
                            {
                                case "Video":
                                    results.Add(VideoBuilder(reader));
                                    break;
                                case "Users":
                                    results.Add(UserBuilder(reader));
                                    break;
                                case "User_Lobby":
                                    results.Add(UserLobbyBuilder(reader));
                                    break;
                                case "Lobby":
                                    results.Add(LobbyBuilder(reader));
                                    break;
                            }

                        }
                        //return reader.Select(r => employeeBuilder(r)).ToList();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Something went wrong: " + ex.Message + " - " + ex.ToString() + " - " + ex.Source + " - " + ex.InnerException + " - " + ex.Data);
                }
            }

            return results;
        }


        private User UserBuilder(SqlDataReader reader)
        {
            return new User
            {
                UserId = int.Parse(reader["userID"].ToString()),
                Username = reader["username"].ToString(),
                Password = reader["password"].ToString(),
                InLobby = int.Parse(reader["inLobby"].ToString()),
                IsReady = int.Parse(reader["isReady"].ToString())
            };
        }

        private Video VideoBuilder(SqlDataReader reader)
        {
            return new Video
            {
                VideoId = int.Parse(reader["videoID"].ToString()),
                Title = reader["title"].ToString(),
                Link = reader["link"].ToString(),
                SyncTime = double.Parse(reader["syncTime"].ToString()),
                IsPaused = int.Parse(reader["isPaused"].ToString())
            };
        }

        private UserLobby UserLobbyBuilder(SqlDataReader reader)
        {
            return new UserLobby
            {
                UserLobbyId = int.Parse(reader["userLobbyID"].ToString()),
                userId = int.Parse(reader["userID"].ToString()),
                LobbyCode = reader["lobbyCode"].ToString(),
                isReady = int.Parse(reader["isReady"].ToString()),
                isLoaded = int.Parse(reader["isLoaded"].ToString()),
            };
        }

        private Lobby LobbyBuilder(SqlDataReader reader)
        {
            return new Lobby
            {
                LobbyCode = reader["lobbyCode"].ToString(),
                InProgress = int.Parse(reader["inProgress"].ToString()),
            };
        }
    }

    public class Lobby
    {
        public string LobbyCode { get; set; }
        public int InProgress { get; set; }
    }

    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int InLobby { get; set; }
        public int IsReady { get; set; }
    }

    public class Video
    {
        public int VideoId { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public double SyncTime { get; set; }
        public int IsPaused { get; set; }
    }

    public class UserLobby
    {
        public int UserLobbyId { get; set; }
        public int userId { get; set; }
        public string LobbyCode { get; set; }
        public int isReady { get; set; }
        public int isLoaded { get; set; }
    }
}