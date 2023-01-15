using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Core.Entities
{
    public class ClientMessageRedis
    {
        public int? SenderId { get; set; }
        public int? ReciverId { get; set; }
        public string? UserMessage { get; set; } // by default null
        public string? MessageTimeStamp { get; set; }
        public string? MessageDateStamp { get; set; }
        public bool MessageSeen { get; set; }
    }
}
