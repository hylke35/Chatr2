using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test2.DB
{

    class Video
    {
        public int VideoId { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public double SyncTime { get; set; }
        public int IsPaused { get; set; }

        // Returns Video object with specified values from DB
        public static  Video Builder(SqlDataReader reader)
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
    }
}
