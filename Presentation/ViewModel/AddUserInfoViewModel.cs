using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.ViewModel
{
    public class AddUserInfoViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string AboutStatus { get; set; }
        public IFormFile? File { get; set; }
    }
}
