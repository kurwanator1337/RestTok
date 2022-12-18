using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestTok.Models
{
    public class AuthorStats
    {
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public int HeartCount { get; set; }
        public int VideosCount { get; set; }
        public int DiggsCount { get; set; }
    }
}
