using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using paen_chat_app_server.SignalRChatHub;

namespace paen_chat_app_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public MessageController(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> SendMessage(string user, string message)
        {
            // more functionallity....

            // sending connected clients a message;
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", user,message);
            return Ok();
        }
    }
}
