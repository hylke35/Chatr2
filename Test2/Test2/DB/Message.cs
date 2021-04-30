using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test2.DB
{
    class Message
    {
        public int MessageId { get; set; }
        public int SenderId { get; set; }
        public string MessageText { get; set; } 

        // Returns User object with specified values from DB
        public static Message Builder(SqlDataReader reader)
        {
            return new Message
            {
                MessageId = int.Parse(reader["messageID"].ToString()),
                SenderId = int.Parse(reader["senderID"].ToString()),
                MessageText = reader["message"].ToString(),
            };
        }
    }
}
