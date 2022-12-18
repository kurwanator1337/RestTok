using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestTok.Models
{
    public class Report
    {
        public Author Author { get; set; }
        public AuthorStats AuthorStats { get; set; }
        public Common Common { get; set; }
        public VideoData VideoData { get; set; }
        public VideoStats VideoStats { get; set; }
    }
}
