using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Core.Some_Data_Classes
{
    public class FetchingSingleUserContacts
    {
        public int ContactId { get; set; }
        public string ContactName { get; set; }
        public string PhoneNumber { get; set; }
        public bool VerifiedContactUser { get; set; }
        public bool BlockContact { get; set; }
        public string AboutStatus { get; set; }
        public string UserImage { get; set; }
        public int UserId { get; set; }
        public bool ConnectedInMessages { get; set; }
        public string groupId { get; set; }
        public bool UserAvailabilityStatus { get; set; }
    }
}
