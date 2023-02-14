using Business_Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.ViewModel.Messages
{
    public class FetchingMessagesForUserViewModel
    {
        public List<ClientMessageRedis> FetchedMessagesList { get; set; }
        public int FetchingMessagesStorageNo { get; set; }
    }
}
