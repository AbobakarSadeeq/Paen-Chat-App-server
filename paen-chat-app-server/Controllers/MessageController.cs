using AutoMapper;
using Business_Core.Entities;
using Business_Core.IServices;
using DataAccess.DataContext_Class;
using DataAccess.Services;
using Hangfire;
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
        public void foo()
        {
            BackgroundJob.Schedule(
                () => Console.WriteLine("Abobakar"),
            TimeSpan.FromDays(2880)); // 2 days
            var client = new BackgroundJobClient();
            var connection = JobStorage.Current.GetConnection();
            var api = JobStorage.Current.GetMonitoringApi();


        }

        [HttpGet("UsingRedis")]
        public async Task<IActionResult> UsingRedis(ClientSingleMessageViewModel viewModel)
        {
            
            var convertToEntity = _mapper.Map<ClientMessageRedis>(viewModel);
            await _redisMessageCacheService.SaveMessageToHash(convertToEntity, viewModel.ConnectionGroupId);
            // await _redisMessageCacheService.ReadingCacheData();
            return Ok();
        }



    }
}
