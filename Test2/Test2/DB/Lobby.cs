using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test2.DB
{
    class Lobby
    {
        public string LobbyCode { get; set; }
        public int InProgress { get; set; }

        // Returns Lobby object with specified values from DB
        public static Lobby Builder(SqlDataReader reader)
        {
            return new Lobby
            {
                LobbyCode = reader["lobbyCode"].ToString(),
                InProgress = int.Parse(reader["inProgress"].ToString()),
            };
        }
    }
}
