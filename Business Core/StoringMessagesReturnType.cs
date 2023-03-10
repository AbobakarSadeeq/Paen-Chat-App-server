using Business_Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Core
{
    public class StoringMessagesReturnType
    {
        public List<Message> StoringAllNewMessagesInDb { get; set; }
        public bool ContactIsInConversationContact { get; set; }
        public StoringMessagesReturnType()
        {
            StoringAllNewMessagesInDb = new List<Message>();
        }
    }
}
