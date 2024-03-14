using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.ViewModel
{
    public class UserViewModel
    {
        public IFormFile File { get; set; }
        public string? FullName { get; set; }
        public string ContactNumber { get; set; }
        public string? AboutStatus { get; set; }

    }
}
