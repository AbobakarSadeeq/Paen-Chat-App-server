using Business_Core.Entities;
using Business_Core.FunctionParametersClasses;
using Business_Core.IServices;
using Business_Core.IUnitOfWork;
using Business_Core.Some_Data_Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Services
{
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMessageRedisCacheService _redisMessageCacheService;



        public MessageService(IUnitOfWork unitOfWork, IMessageRedisCacheService redisMessageCacheService)
        {
            _unitOfWork = unitOfWork;
            _redisMessageCacheService = redisMessageCacheService;
        }

        // below messages is fetching from db
        public async Task<List<Message>> GetSingleConversationMessagesAllListFromDbAsync(int user1, int user2)
        {
            return await _unitOfWork._messageRepository.FetchingSingleConversationAllMessagesAsync(user1, user2);
            
        }


        public async Task<FetchingMessagesForUser> GetSingleConversationAllMessagesFromRedisAndDbAsync(SingleConversationMessagesParams fetchingSpecificMessageParams)
        {

            // fetchingDataStorage = 1 => fetching data from the new list from redis 
            // fetchingDataStorage = 2 => fetching data from the old list from redis 
            // fetchingDataStorage = 3 => fetching data from the ms-sql-database 
            // when change storage 
            // reset lastCOunt
            // reset scroll to zero
            // if lastCOunt 30 and storage not change then change scroll only and lastCOunt to 0 again

            // here i am updating the unread messages
            if (fetchingSpecificMessageParams.UnReadMessages > 0)
            {
                await _redisMessageCacheService.UpdateUnReadMessageMarkedReadedAsync(fetchingSpecificMessageParams.groupId);
            }


            var fetchingUserMessages = await _redisMessageCacheService.FetchingSingleConversationUsersMessagesFromRedisAsync(fetchingSpecificMessageParams);
            // fetching messages from redis
            if (fetchingUserMessages.FetchedMessagesList.Count > 0)
            {
                return fetchingUserMessages;

            }

            var fetchingSingleConversationAllMessagesFromDb = await GetSingleConversationMessagesAllListFromDbAsync(fetchingSpecificMessageParams.user1, fetchingSpecificMessageParams.user2);

            fetchingUserMessages = await _redisMessageCacheService.FetchingSingleConversationUsersMessagesFromDb(fetchingSpecificMessageParams, fetchingSingleConversationAllMessagesFromDb);
            if (fetchingUserMessages.FetchedMessagesList.Count + fetchingSpecificMessageParams.lastMessagesCount == 30)
                fetchingUserMessages.LastMessagesCount = 0;
            else
                fetchingUserMessages.LastMessagesCount = fetchingUserMessages.FetchedMessagesList.Count;


            return fetchingUserMessages;

            // by default scrolling will be 1
            // when FetchingMessagesStorageNo return -1 then it means you have to tell on client side to user s that all messages has been delivered and no more messages found here in redis and db here.

            // * when data is fetched check it on client side Arr.Length === scrollNo * 30 then return scrollNo++ and fetch data from only that storage place than.

            // by default lastMessageCount will become 0 when 30 or more than 30 messages is return to the client.
        }


        public async Task StoringUsersMessagesAsync(List<Message> usersAllMessagesList)
        {
            await _unitOfWork._messageRepository.StoringUsersMessages(usersAllMessagesList);
            await _unitOfWork.BulkCommitAsync();
        }
    }
}
