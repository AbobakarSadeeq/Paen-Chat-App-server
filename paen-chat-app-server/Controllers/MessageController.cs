using AutoMapper;
using Business_Core.Entities;
using Business_Core.IServices;
using DataAccess.DataContext_Class;
using DataAccess.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json.Linq;
using paen_chat_app_server.Redis_data_models;
using paen_chat_app_server.Redis_Extensions;
using paen_chat_app_server.SignalRChatHub;
using Presentation.ViewModel.Messages;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace paen_chat_app_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        //private readonly IHubContext<ChatHub> _hubContext;
        //private readonly DataContext _dataContext;
        //private readonly IRedisCacheService _redisCacheService;

        //public MessageController(IHubContext<ChatHub> hubContext,
        //    DataContext dataContext, IRedisCacheService redisCacheService)
        //{
        //    _hubContext = hubContext;
        //    _dataContext = dataContext;
        //    _redisCacheService = redisCacheService;
        //}

        private readonly IMessageService _messageService;
        private readonly IMapper _mapper;
        private readonly IMessageRedisCacheService _redisMessageCacheService;
        public MessageController(IMessageService messageService, IMapper mapper, IMessageRedisCacheService redisMessageCacheService)
        {
            _messageService = messageService;
            _mapper = mapper;
            _redisMessageCacheService = redisMessageCacheService;
        }

        //[HttpGet]
        //public async Task<IActionResult> SendMessage(string user, string message)
        //{


        //    // sending connected clients a message;
        //    await _hubContext.Clients.All.SendAsync("ReceiveMessage", user,message);
        //    return Ok();
        //}


        //[HttpPost]
        //public async Task<IActionResult> StoringMessage(List<ClientSingleMessageViewModel> viewModels)
        //{
        //    if (viewModels.Count == 0)
        //        return Ok();

        //    List<Message> userMessages = new List<Message>();
        //    foreach (var item in viewModels)
        //    {
        //        //DateTime myDate = DateTime.ParseExact(item.MessageDateStamp + " " + item.MessageTimeStamp, "yyyy-MM-dd HH:mm",
        //        //                       System.Globalization.CultureInfo.InvariantCulture);
        //        Message message = new Message();
        //        message.SenderId = item.SenderId;
        //        message.ReciverId = item.ReciverId;
        //        message.UserMessage = item.UserMessage;
        //        message.MessageSeen = item.MessageSeen;

        //        message.Created_At = DateTime.ParseExact(item.MessageDateStamp + " " + item.MessageTimeStamp, "M/dd/yyyy h:mm tt", null);
        //        userMessages.Add(message);

        //    }
        //    await _dataContext.Messages.AddRangeAsync(userMessages);
        //    await _dataContext.SaveChangesAsync();

        //    return Ok();
        //}

        [HttpPost]
        public async Task<IActionResult> StoringMessage(ClientSingleMessageViewModel clientMessageViewModel)
        {
            var storingMessagesToHash = await _redisMessageCacheService.SaveMessageToHashAsync(clientMessageViewModel.clientMessageRedis, clientMessageViewModel.GroupId);
            // above line is storing data in db after 2 days passed.
            // above line storing data in recently message hash in redis.
            // above line stroing data in userAllMessages hash in redis.
            // above line is making the hash empty of recentlyMessage when it is stored inside the userAllMessages hash in redis.
            if(storingMessagesToHash.Count > 0)
            {
                await _messageService.StoringUsersMessagesAsync(storingMessagesToHash);
            }

            return Ok();
        }
      

        [HttpGet("UsingRedis")]
        public async Task<IActionResult> UsingRedis(ClientSingleMessageViewModel viewModel)
        {
            
            var convertToEntity = _mapper.Map<ClientMessageRedis>(viewModel);
          //  await _redisMessageCacheService.SaveMessageToHashAsync(convertToEntity, viewModel.ConnectionGroupId);
            // await _redisMessageCacheService.ReadingCacheData();
            return Ok();
        }



    }
}
