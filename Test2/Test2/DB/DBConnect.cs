using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
using Test2.DB;

namespace Test2
{
    public class DBConnect
    {
        // Connection String of SQL Server
        string asyncConnectionString = new SqlConnectionStringBuilder(@"Data Source=51.116.224.130,1433;Initial Catalog=ChatrTestDB;User ID=chatr;Password=TestPassword1!;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False").ToString();

        public string GetConnectionString()
        {
            return asyncConnectionString;
        }

        // Executes a given non query asynchonous 
        /// <summary>
        /// Executes a given non query asynchonous 
        /// </summary>
        /// <param name="query">Used to specify the query.</param>
        /// <param name="addwithvalue">Used to specify variables (optional) that are needed to execute the query.</param>
        public async void RunQueryAsync(string query, IDictionary<string, object> addwithvalue = null)
        {
            using (SqlConnection connection = new SqlConnection(asyncConnectionString))
            {
                // Open asynchonrous connection
                await connection.OpenAsync();
                SqlCommand command = connection.CreateCommand();

                try
                {
                    command.CommandText = query;

                    if (addwithvalue != null)
                    {
                        // Adds values of query if necessary
                        foreach (KeyValuePair<string, object> kvp in addwithvalue)
                        {
                            command.Parameters.AddWithValue(kvp.Key, kvp.Value);
                        }
                    }
                    // Executes specific command asynchronously
                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);

                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Something went wrong:" + ex.Message);
                }
            }
        }

        // Executes a given query asynchonous and returns its data.
        /// <summary>
        /// Executes a given query asynchonous and returns its data.
        /// </summary>
        /// <param name="query">Used to specify the query.</param>
        /// <param name="table">Used to specify the database table.</param>
        /// <param name="addwithvalue">Used to specify variables (optional) that are needed to execute the query.</param>
        /// <returns>Returns a Task that contains an Enumrable object (User, Video, UserLobby, Lobby).</returns>
        public async Task<IEnumerable<object>> DataReader(string query, string table, IDictionary<string, object> addwithvalue = null)
        {
            var results = new List<object>();
            using (SqlConnection connection = new SqlConnection(asyncConnectionString))
            {
                // Opens asynchonous connection
                await connection.OpenAsync();
                SqlCommand command = connection.CreateCommand();

                try
                {
                    command.CommandText = query;
                    if (addwithvalue != null)
                    {
                        foreach (KeyValuePair<string, object> kvp in addwithvalue)
                        {
                            // Adds values of query if necessary
                            command.Parameters.AddWithValue(kvp.Key, kvp.Value);
                        }
                    }
                    // Returns results of table into a List
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            switch (table)
                            {
                                case "Video":
                                    results.Add(Video.Builder(reader));
                                    break;
                                case "Users":
                                    results.Add(User.Builder(reader));
                                    break;
                                case "User_Lobby":
                                    results.Add(UserLobby.Builder(reader));
                                    break;
                                case "Lobby":
                                    results.Add(Lobby.Builder(reader));
                                    break;
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Something went wrong: " + ex.Message + " - " + ex.ToString() + " - " + ex.Source + " - " + ex.InnerException + " - " + ex.Data);
                }
            }

            return results;
        }
    }
}