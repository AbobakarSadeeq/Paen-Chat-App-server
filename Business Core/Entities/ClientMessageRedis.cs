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
        public int? ReceiverId { get; set; }
        public string? UserMessage { get; set; } // by default null
        public string? MessageTimeStamp { get; set; } = DateTime.Now.ToString("hh:mm tt");
        public string? MessageDateStamp { get; set; } = DateTime.Now.ToShortDateString();
        public bool MessageSeen { get; set; }
    }
}
