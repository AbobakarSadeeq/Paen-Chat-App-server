using Business_Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.ViewModel.Messages
{
    public class ClientSingleMessageViewModel
    {
        public ClientMessageRedis  clientMessageRedis { get; set; }
        public string GroupId { get; set; } = "";
    }
}
