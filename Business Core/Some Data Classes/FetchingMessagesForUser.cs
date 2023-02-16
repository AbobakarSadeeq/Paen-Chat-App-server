using Business_Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Core.Some_Data_Classes
{
    public class FetchingMessagesForUser
    {
        public List<ClientMessageRedis> FetchedMessagesList { get; set; }
        public int FetchingMessagesStorageNo { get; set; }
    }
}
