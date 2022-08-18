using DataAccess.DataContext_Class;
using DataAccess.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace paen_chat_app_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly DataContext _dataContext;
        public TestController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpPost]
        public async Task<IActionResult> Post(Test name)
        {
           await _dataContext.Tests.AddAsync(name);
           await _dataContext.SaveChangesAsync();
            return Ok();
        }

    }
}
