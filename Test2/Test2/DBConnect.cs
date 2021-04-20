using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Test2
{
    public class DBConnect
    {
        string asyncConnectionString = new SqlConnectionStringBuilder(@"Data Source=DESKTOP-SE6V62U,1433;Initial Catalog=ChatrTestDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False").ToString();

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
                    //var isPaused = 1;
                    //if (status == "playing")
                    //{
                    //    isPaused = 0;
                    //}
                    //else if (status == "paused")
                    //{
                    //    isPaused = 1;
                    //}

                    command.CommandText = query;

                    if (addwithvalue != null)
                    {
                        foreach(KeyValuePair<string, object> kvp in addwithvalue)
                        {
                            command.Parameters.AddWithValue(kvp.Key, kvp.Value);
                        }
                    }

                    await command.ExecuteNonQueryAsync();

                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Something went wrong: {0}", ex.Message);
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
                            if (table == "Video")
                            {
                                results.Add(videoBuilder(reader));
                            } else if (table == "Users")
                            {
                                results.Add(userBuilder(reader));
                            }
                 
                        }
                        //return reader.Select(r => employeeBuilder(r)).ToList();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Something went wrong: {0}", ex.Message);
                }
            }

            return results;
        }


        private User userBuilder(SqlDataReader reader)
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

        private Video videoBuilder(SqlDataReader reader)
        {
            return new Video
            {
                VideoId = int.Parse(reader["videoID"].ToString()),
                Link = reader["link"].ToString(),
                RunTime = reader["runTime"].ToString(),
                CurrentTime = reader["currentTime"].ToString(),
                IsPaused = bool.Parse(reader["isPaused"].ToString())
            };
        }

        /*        public object ShowDataInGridView(string Query_)
                {
                    SqlDataAdapter dr = new SqlDataAdapter(Query_, ConnectionString);
                    DataSet ds = new DataSet();
                    dr.Fill(ds);
                    object dataum = ds.Tables[0];
                    return dataum;
                }*/
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
        public string Link { get; set; }
        public object RunTime { get; set; }
        public object CurrentTime { get; set; }
        public bool IsPaused { get; set; }
    }
}