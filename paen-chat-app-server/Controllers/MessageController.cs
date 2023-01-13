using DataAccess.DataContext_Class;
using DataAccess.Entities;
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
using System.Text.Json;
using System.Text.Json.Nodes;

namespace paen_chat_app_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly DataContext _dataContext;
        private readonly IRedisCacheService _redisCacheService;

        public MessageController(IHubContext<ChatHub> hubContext,
            DataContext dataContext, IRedisCacheService redisCacheService)
        {
            _hubContext = hubContext;
            _dataContext = dataContext;
            _redisCacheService = redisCacheService;
        }

        [HttpGet]
        public async Task<IActionResult> SendMessage(string user, string message)
        {
            

            // sending connected clients a message;
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", user,message);
            return Ok();
        }


        [HttpPost]
        public async Task<IActionResult> StoringMessage(List<ClientSingleMessageViewModel> viewModels)
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

                message.Created_At = DateTime.ParseExact(item.MessageDateStamp + " " + item.MessageTimeStamp, "M/dd/yyyy h:mm tt", null);
                userMessages.Add(message);
                
            }
            await _dataContext.Messages.AddRangeAsync(userMessages);
            await _dataContext.SaveChangesAsync();

            return Ok();
        }


        [HttpGet("UsingRedis")]
        public async Task<IActionResult> UsingRedis(ClientSingleMessageViewModel viewModel)
        {
            //await redisCache.SetValueToRedisAsync<string>("1", "from .net"); // make the serverices for that here and register that service in DI and MS recommended to not separatly and dont use the extension method here for that
            //var gettingDataFromRedis = await redisCache.GetRecordAsync<string>("1");

            await _redisCacheService.SaveMessageToHash(viewModel, "GroupId0");

            //var employeeDataList = new List<Employee>()
            //{
            //    new Employee() { Id = 1, Name = "abc", Age= 20},
            //    new Employee() { Id =2, Name = "dsfgabc", Age= 20},
            //    new Employee() { Id = 3, Name = "dfg", Age= 21230},
            //    new Employee() { Id = 4, Name = "sdfg", Age= 2230},
            //    new Employee() { Id = 5, Name = "dsfg", Age= 220},
            //    new Employee() { Id = 6, Name = "dfsg", Age= 2230},
            //    new Employee() { Id = 7, Name = "re", Age= 20321},
            //    new Employee() { Id = 8, Name = "tretwrt", Age= 202},
            //};

            //var conertingToJsonString = JsonSerializer.Serialize(employeeDataList);
            //JArray rss = JArray.Parse(conertingToJsonString);
            //var newEmployee = new Employee() { Id = 9, Name = "Abobakar", Age = 10 };
            //var convertSingleObjToJson = JsonSerializer.Serialize(newEmployee);
            //rss.Add(convertSingleObjToJson);
            
            //var convertAgain = rss.ToString();


            //var convertingToEmployeesListNormal = JsonSerializer.Deserialize<List<Employee>>(convertAgain);
           // var addingAnotherEmployeeToString = J



            return Ok();
        }



        }
}
