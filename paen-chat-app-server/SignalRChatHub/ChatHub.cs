using Business_Core.Entities;
using DataAccess.DataContext_Class;
using Microsoft.AspNetCore.SignalR;
using Presentation.ViewModel.Messages;
using Presentation.ViewModel.UserHub;
using Business_Core.IServices;
using Business_Core.FunctionParametersClasses;
using AutoMapper;
using Business_Core.Some_Data_Classes;

namespace paen_chat_app_server.SignalRChatHub
{
    public class ChatHub : Hub
    {
        private readonly IMessageService _messageService;
        private readonly IMapper _mapper;
        private readonly IMessageRedisCacheService _redisMessageCacheService;
        private readonly IUserRedisCacheService _redisUserCacheService;
        private readonly IContactService _contactService;
        public ChatHub(
            IMessageService messageService,
            IMapper mapper,
            IMessageRedisCacheService redisMessageCacheService,
            IContactService contactService,
            IUserRedisCacheService redisUserCacheService)
        {
            _messageService = messageService;
            _mapper = mapper;
            _redisMessageCacheService = redisMessageCacheService;
            _redisUserCacheService = redisUserCacheService;
            _contactService = contactService;
        }


        //  When connection is ON then first method inside the hub-class this method execute.
        public async Task OnCustomConnectedAsync(string userId)
        {
            // whose is connencted or become logged-in then add to redis about user become online;
            List<string> userConnectedContactsGroupId = await _redisUserCacheService.GetUserConnectedContactsGroupIdFromRedisHashInValidFormateAsync(userId);
            foreach (var singleGroupId in userConnectedContactsGroupId)
            {
                await Clients.Group(singleGroupId).SendAsync("UserBecomeOnline", singleGroupId, userId);

            }

        }

        //  When connection is OFF then first method inside the hub-class this method execute.

        public async override Task OnDisconnectedAsync(Exception? exception)
        {
           var queryString =  Context.GetHttpContext().Request.QueryString.Value;
           int startIndex = queryString.IndexOf('=') + 1;
           int endIndex = queryString.Length;
           string userId = queryString.Substring(startIndex, endIndex - startIndex);

            // removing the id from set because of user become offline
            await _redisUserCacheService.UserAvailabilityStatusRemoveFromSetRedis(userId);

            // remove the hash data as well and that userId:[groupId]
            List<string> userConnectedContactsGroupId = await _redisUserCacheService.GetUserConnectedContactsGroupIdFromRedisHashInValidFormateAsync(userId);

            foreach (var singleGroupId in userConnectedContactsGroupId)
            {
                await Clients.Group(singleGroupId).SendAsync("UserBecomeOffline", singleGroupId, userId);
            }

            // removed the user all connected contacts from redis hash ds
            await _redisUserCacheService.RemoveUserConnectedContactsGroupIdFromRedisHashAsync(userId);


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

            // call the client side function for to show it to the receiver user

            // if user is not logged in then store the data directly on redis and messageSeen become zero
            if(await _redisUserCacheService.UserAvailabilityStatusChecking(singleMessage.clientMessageRedis.ReceiverId.ToString()) == false)
            {
                // if user-offline then message seen is zero
               await StoreMessageOnRedis(singleMessage);
            } 
 
            await Clients.Group(singleMessage.GroupId).SendAsync("ReceiveingSenderMessageFromConnectedContactUser", singleMessage);
        }

        // user become online and founded then this will be inoke from client
        public async Task StoreMessageOnRedis(ClientSingleMessageViewModel singleMessage)
        {
            await StoringMessageAsync(singleMessage);
            // sending back to sender about the messages-seen has been updated .
            await Clients.Group(singleMessage.GroupId).SendAsync("SendedMessageSeenUpdated", singleMessage);


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

        public async Task TickReadedForSenderOfAllSendedMessages(string groupId, int senderId)
        {
            await Clients.Group(groupId).SendAsync("MakeItReadedAllUnReadMessagesFromSenderSide", senderId);

        }

        public async Task ContactBlockingAndUnlocking(string groupId, int userIdWhoBlockTheContact)
        {
            await Clients.Group(groupId).SendAsync("BlockingOrUnlockingContactLive", groupId, userIdWhoBlockTheContact);
        }


        //public async Task RemoveUserFromGroup(string groupName)
        //{
        //    await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        //    await Clients.Group(groupName).SendAsync("UserLeftGroup", $"{Context.ConnectionId} has left the group {groupName}.");
        //}








        //public async Task UserTryingToDisconnecting(List<string> groupsName, string disconnectingUserId)
        //{
        //    // those groupsName will be used for to send request to those function whose are same and to update their array state
        //    // remove that id from js array of other user array list
        //    await  Clients.Groups(groupsName).SendAsync("RemoveDisconnectedUserDataFromArray", disconnectingUserId);

        //}







    }

}

