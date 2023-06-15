using Business_Core.Entities;
using DataAccess.DataContext_Class;
using Microsoft.AspNetCore.SignalR;
using Presentation.ViewModel.Messages;
using Presentation.ViewModel.UserHub;
using Business_Core.IServices;
using Business_Core.FunctionParametersClasses;
using AutoMapper;

namespace paen_chat_app_server.SignalRChatHub
{
    public class ChatHub : Hub
    {
        private readonly IMessageService _messageService;
        private readonly IMapper _mapper;
        private readonly IMessageRedisCacheService _redisMessageCacheService;
        private readonly IContactService _contactService;
        public ChatHub(
            IMessageService messageService,
            IMapper mapper,
            IMessageRedisCacheService redisMessageCacheService,
            IContactService contactService)
        {
            _messageService = messageService;
            _mapper = mapper;
            _redisMessageCacheService = redisMessageCacheService;
            _contactService = contactService;
        }
        


        public async Task ConnectingSingleUserOfSingleContactWithTheirUniqueGroupIdForRealTimeMessaging(string groupName, string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            // below code will be execute the front-side code for you or front-side function and for now not required yeah!
            //await Clients.Group(groupName).SendAsync("UserSingleContactIsConnectedWithTheirsGroup", userId, groupName);


        }



        public async Task SendMessageToGroup(ClientSingleMessageViewModel singleMessage)
        {
           // await Clients.Group(singleMessage.GroupId).SendAsync("SendMessage", singleMessage.clientMessageRedis);
            // now i have to call the redis here to store the message in redis databaase
            await StoringMessageAsync(singleMessage);

            // call the client side function for to show it to the receiver user
 
            await Clients.Group(singleMessage.GroupId).SendAsync("ReceiveingSenderMessageFromConnectedContactUser", singleMessage);
        }

        public async Task StoringMessageAsync(ClientSingleMessageViewModel clientMessageViewModel)
        {
            var storingAllNewMessagesInDb = await _redisMessageCacheService.SaveMessagesInRedisAsync(clientMessageViewModel.clientMessageRedis, clientMessageViewModel.GroupId);

            // need to have a signal about is that

            // above line is storing data in db after 2 days passed.
            // above line storing data in new message list in redis.
            // above line stroing data in old list in redis.
            // above line is making the new list empty and when it is stored inside the old list in redis.

            if (storingAllNewMessagesInDb.StoringAllNewMessagesInDb.Count > 0)
            {
                await _messageService.StoringUsersMessagesAsync(storingAllNewMessagesInDb.StoringAllNewMessagesInDb);
                // here i am using the bulk insert of EF core which will be store alot of list data in fast way.
            }

            if (storingAllNewMessagesInDb.ContactIsInConversationContactList == true)
            {
                await _contactService.AddConversationContactToConversationListAsync(clientMessageViewModel.GroupId);
            }
        }




        public async Task RemoveUserFromGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("UserLeftGroup", $"{Context.ConnectionId} has left the group {groupName}.");
        }

        //  When connection is ON then first method inside the hub-class this method execute.
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        

        


        public async Task UserTryingToDisconnecting(List<string> groupsName, string disconnectingUserId)
        {
            // those groupsName will be used for to send request to those function whose are same and to update their array state
            // remove that id from js array of other user array list
            await  Clients.Groups(groupsName).SendAsync("RemoveDisconnectedUserDataFromArray", disconnectingUserId);
        
        }


        public override Task OnDisconnectedAsync(Exception? exception)
        {
            // whenever user disconnected then send the message to other user whose is connected with him/her





            return base.OnDisconnectedAsync(exception);
        }





    }

}

