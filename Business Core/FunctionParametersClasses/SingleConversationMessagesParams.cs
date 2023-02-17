using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Core.FunctionParametersClasses
{
    public class SingleConversationMessagesParams
    {
        public int currentScrollMessangeNumber { get; set; }
        public int fetchingMessagesStorageNo { get; set; }
        public string groupId { get; set; }
        public int user1 { get; set; }
        public int user2 { get; set; }
        public int lastMessagesCount { get; set; } = 0; //  

    }
}
