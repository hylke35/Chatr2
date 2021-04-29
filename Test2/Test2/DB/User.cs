using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test2.DB
{
    class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int InLobby { get; set; }
        public int IsReady { get; set; }

        // Returns User object with specified values from DB
        public static User Builder(SqlDataReader reader)
        {
            return new User
            {
                UserId = int.Parse(reader["userID"].ToString()),
                Username = reader["userrname"].ToString(),
                Password = reader["password"].ToString(),
                InLobby = int.Parse(reader["inLobby"].ToString()),
                IsReady = int.Parse(reader["isReady"].ToString())
            };
        }
    }
}
