using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.ViewModel.Contact
{
    public class AddContactLiveViewModel
    {
        public int ContactId { get; set; }
        public string? ContactName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? UserImage { get; set; }
        public bool VerifiedContactUser { get; set; }
        public bool BlockContact { get; set; }
        public bool BlockContactByConnectedUser { get; set; } = false;
        public int UserId { get; set; }
        public string? GroupId { get; set; }
        public bool ConnectedInMessages { get; set; }
        public int CountUnSeenMessages { get; set; } = 0;
        public string LastMessageOfSingleContact { get; set; } = "";
        public string AboutStatus { get; set; }
        public bool UserAvailabilityStatus { get; set; } = true;


    }
}
