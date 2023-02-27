using AutoMapper;
using Business_Core.Entities;
using Business_Core.FunctionParametersClasses;
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

        private readonly IMessageService _messageService;
        private readonly IMapper _mapper;
        private readonly IMessageRedisCacheService _redisMessageCacheService;
        public MessageController(IMessageService messageService, IMapper mapper, IMessageRedisCacheService redisMessageCacheService)
        {
            _messageService = messageService;
            _mapper = mapper;
            _redisMessageCacheService = redisMessageCacheService;
        }

        [HttpPost]
        public async Task<IActionResult> StoringMessage(ClientSingleMessageViewModel clientMessageViewModel)
        {
          var storingAllNewMessagesInDb =  await _redisMessageCacheService.SaveMessagesInRedisAsync(clientMessageViewModel.clientMessageRedis, clientMessageViewModel.GroupId);

            // above line is storing data in db after 2 days passed.
            // above line storing data in new message list in redis.
            // above line stroing data in old list in redis.
            // above line is making the new list empty and when it is stored inside the old list in redis.

            if (storingAllNewMessagesInDb.Count > 0)
            {
                await _messageService.StoringUsersMessagesAsync(storingAllNewMessagesInDb);
                // inseration issues of million at the same time so, have to divide it into sub-data or sub-list like when 10k element stored in list then do one transaction there.
            }
            return Ok();
        }
      

        [HttpGet]
        public async Task<IActionResult> GetMessagesOfSingleConversation(SingleConversationMessagesParams fetchingSpecificMessageParams)
        {
            var fetchingData = await _redisMessageCacheService.FetchingSingleConversationUsersMessagesFromRedis(fetchingSpecificMessageParams);
            return Ok();
        }




    }
}
