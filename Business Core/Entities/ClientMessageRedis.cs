﻿using System;
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
        public string? MessageTimeStamp { get; set; }
        public string? MessageDateStamp { get; set; }
        public int MessageSeen { get; set; } // 0 => offline, 1 => online, 3 => specific connected with page
    }
}
