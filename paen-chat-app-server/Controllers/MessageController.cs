using DataAccess.DataContext_Class;
using DataAccess.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using paen_chat_app_server.SignalRChatHub;
using Presentation.ViewModel.Messages;

namespace paen_chat_app_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly DataContext _dataContext;

        public MessageController(IHubContext<ChatHub> hubContext, DataContext dataContext)
        {
            _hubContext = hubContext;
            _dataContext = dataContext;
        }

        [HttpGet]
        public async Task<IActionResult> SendMessage(string user, string message)
        {
            

            // sending connected clients a message;
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", user,message);
            return Ok();
        }


        [HttpPost]
        public async Task<IActionResult> StoringMessage(List<MessagesViewModel> viewModels)
        {
            if (viewModels.Count == 0)
                return Ok();

            List<Message> userMessages = new List<Message>();
            foreach (var item in viewModels)
            {
                //DateTime myDate = DateTime.ParseExact(item.MessageDateStamp + " " + item.MessageTimeStamp, "yyyy-MM-dd HH:mm",
                //                       System.Globalization.CultureInfo.InvariantCulture);
                Message message = new Message();
                message.SenderId = item.SenderId;
                message.ReciverId = item.ReciverId;
                message.UserMessage = item.UserMessage;
                message.MessageSeen = item.MessageSeen;
                message.Message_Type = item.Message_Type;
                message.Created_At = DateTime.ParseExact(item.MessageDateStamp+ " " + item.MessageTimeStamp, "M/dd/yyyy h:mm tt", null);
                userMessages.Add(message);
                
            }
            await _dataContext.Messages.AddRangeAsync(userMessages);
            await _dataContext.SaveChangesAsync();

            return Ok();
        }



    }
}
