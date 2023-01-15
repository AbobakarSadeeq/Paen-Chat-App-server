using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Business_Core.Entities
{
    public class AddUserInfo
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string AboutStatus { get; set; }
        public Microsoft.AspNetCore.Http.IFormFile? File { get; set; }
        public string? PublicId { get; set; }
        public string? ProfilePhotoUrl { get; set; }


    }
}
