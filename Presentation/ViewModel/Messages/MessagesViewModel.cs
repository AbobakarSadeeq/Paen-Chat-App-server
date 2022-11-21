using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.ViewModel.Messages
{
    public class MessagesViewModel
    {
        public int? SenderId { get; set; }
        public int? ReciverId { get; set; }
        public string? Message_Type { get; set; }
        public string? UserMessage { get; set; } // by default null
        public string? MessageTimeStamp { get; set; }
        public string? MessageDateStamp { get; set; }
        public bool MessageSeen { get; set; }
    }
}
