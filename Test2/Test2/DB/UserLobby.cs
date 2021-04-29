using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test2.DB
{
    class UserLobby
    {
        public int UserLobbyId { get; set; }
        public int userId { get; set; }
        public string LobbyCode { get; set; }
        public int isReady { get; set; }
        public int isLoaded { get; set; }

        // Returns UserLobby object with specified values from DB
        public static UserLobby Builder(SqlDataReader reader)
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
    }
}
