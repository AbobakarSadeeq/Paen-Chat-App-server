using Business_Core.Entities;
using Business_Core.IServices;
using StackExchange.Redis;
using SpanJson;

namespace DataAccess.Services
{
    public class MessageRedisCacheService : IMessageRedisCacheService
    {
        private readonly IConnectionMultiplexer _redis;
        public MessageRedisCacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }
      

        public async Task<List<Message>> SaveMessageToHashAsync(ClientMessageRedis clientMessage, string groupId)
        {
            var redisDb = _redis.GetDatabase();
            await CreateHashesAndAssignTimeSpanForUpdatingStoragesInRedisAsync(redisDb);
            var getGroupIdData = await redisDb.HashGetAsync("RecentlyUsersMessagesStorage", groupId);


            List<ClientMessageRedis> singleGroupMessage = ConvertingStringToObjects(getGroupIdData);
            singleGroupMessage.Add(clientMessage);

            // again now serializing it for to store that new data in redis hash
            string newMessageAddedToHash = ConvertingMultipleObjectsToString(singleGroupMessage);

            HashEntry[] insertDataToRedisHash = {

            new HashEntry(groupId, newMessageAddedToHash),

            };
            await redisDb.HashSetAsync("RecentlyUsersMessagesStorage", insertDataToRedisHash);

            // date string == date string tommarow test done!!!!!
            var fetchingMessagesShiftingDateTime = await redisDb.StringGetAsync("ShiftingNewMessageDataTimeSpan");
            var currentDate = DateTime.Today.ToString("d");
            if(currentDate == fetchingMessagesShiftingDateTime)
            {
                var assigningNewDate = DateTime.Parse(fetchingMessagesShiftingDateTime.ToString()).AddDays(2).ToString("d");
                await redisDb.StringSetAsync("ShiftingNewMessageDataTimeSpan", assigningNewDate.ToString());
                return  await AddRecentlyMessagesHashToUsersAllMessagesStorageHashAsync(redisDb);

            }
            return new List<Message>();


        }

        // below algorithms thats calling on upper methods
        private async Task CreateHashesAndAssignTimeSpanForUpdatingStoragesInRedisAsync(IDatabase redisDb)
        {
            if(await redisDb.HashLengthAsync("RecentlyUsersMessagesStorage") == 0) // hlen == O(1)
            {
                HashEntry[] insertDataToRedisHash = {
                   new HashEntry("CreateHash", "Default Value")
                };
                await redisDb.HashSetAsync("RecentlyUsersMessagesStorage", insertDataToRedisHash);
                await redisDb.HashDeleteAsync("RecentlyUsersMessagesStorage", "CreateHash"); // the hash will became empty now.
            }

            if (await redisDb.HashLengthAsync("UsersAllMessagesDataStorage") == 0) // hlen == O(1)
            {
                HashEntry[] insertDataToRedisHash = {
                   new HashEntry("CreateHash", "Default Value")
                };
                await redisDb.HashSetAsync("UsersAllMessagesDataStorage", insertDataToRedisHash);
                await redisDb.HashDeleteAsync("UsersAllMessagesDataStorage", "CreateHash"); // the hash will became empty now.

            }

            if (redisDb.StringGet("ShiftingNewMessageDataTimeSpan").IsNull)
            {
                await redisDb.StringSetAsync("ShiftingNewMessageDataTimeSpan", DateTime.Today.ToString("d"));
            }
        }

        private List<ClientMessageRedis> ConvertingStringToObjects(string multipleMessages)
        {
            if (multipleMessages == null)
                return new List<ClientMessageRedis>();

            var convertingString = JsonSerializer.Generic.Utf16.Deserialize<List<ClientMessageRedis>>(multipleMessages);
            return convertingString;
        }
        private string ConvertingMultipleObjectsToString(List<ClientMessageRedis> usersMessagesList)
        {
            if (usersMessagesList == null)
                return string.Empty;

            var convertingObject = JsonSerializer.Generic.Utf16.Serialize(usersMessagesList);
            return convertingObject;
        }


        private async Task<List<Message>> AddRecentlyMessagesHashToUsersAllMessagesStorageHashAsync(IDatabase selectingDb)
        {

            // get the recently data first
            List<Message> RecentlyNewAllConversationMessagesForStoreInDb = new List<Message>(); // => this list is going to send or store data on db and that will be sended to message service.

            var getRecentlyAllUsersMessages = await selectingDb.HashKeysAsync("RecentlyUsersMessagesStorage");
            foreach (var singleConversationGroupKey in getRecentlyAllUsersMessages)
            {

                var getSingleConversationAllNewMessagesData = await selectingDb.HashGetAsync("RecentlyUsersMessagesStorage", singleConversationGroupKey);

                List<Message> singleConversationNewMessages = ConvertRedisStoreSingleMessageIntoDbMessageFormate(getSingleConversationAllNewMessagesData);

                RecentlyNewAllConversationMessagesForStoreInDb.AddRange(singleConversationNewMessages);

                 await StoringAllSingleConversationMessagesToHashRedisStorageAsync(selectingDb, singleConversationGroupKey, getSingleConversationAllNewMessagesData);

                await DeleteSingleConversationRecentlyUsersMessageFromRedisHashStorage(singleConversationGroupKey, selectingDb);
            }

            return RecentlyNewAllConversationMessagesForStoreInDb;
        }
        // store that new message on db as well => done
        private List<Message> ConvertRedisStoreSingleMessageIntoDbMessageFormate(string singleConversationAllNewMessage)
        {
            var convertStringIntoRedisMessageFormate = ConvertingStringToObjects(singleConversationAllNewMessage);

            List<Message> RecentlyNewMessagesOfSingleConversation = new List<Message>();

            foreach (var clientMessageRedis in convertStringIntoRedisMessageFormate)
            {
                Message singleMessage = new Message
                {
                    SenderId = clientMessageRedis.SenderId,
                    ReciverId = clientMessageRedis.ReceiverId,
                    UserMessage = clientMessageRedis.UserMessage,
                    MessageSeen = clientMessageRedis.MessageSeen,
                    Created_At = DateTime.Parse(clientMessageRedis.MessageDateStamp + " " + clientMessageRedis.MessageTimeStamp)
                };
                RecentlyNewMessagesOfSingleConversation.Add(singleMessage);
            }
           
            return RecentlyNewMessagesOfSingleConversation;
        }

        // store that in oldMessages as well in redis. => done
        private async Task StoringAllSingleConversationMessagesToHashRedisStorageAsync(IDatabase redisDatabase, string conversationGroupIdKey, string allRecentMessagesOfSingleConversation)
        {
            var fetchAllStoredMessagesOfSingleConversation = await redisDatabase.HashGetAsync("UsersAllMessagesDataStorage", conversationGroupIdKey);

            List<ClientMessageRedis> recentlySingleConversationAllMessages = ConvertingStringToObjects(allRecentMessagesOfSingleConversation);

            List<ClientMessageRedis> singleConversationUserAllRecentlyAndStoredMessages = ConvertingStringToObjects(fetchAllStoredMessagesOfSingleConversation);

            singleConversationUserAllRecentlyAndStoredMessages.AddRange(recentlySingleConversationAllMessages);

            string convertingAllSingleConversationMessagesToStringToStoreInHash = ConvertingMultipleObjectsToString(singleConversationUserAllRecentlyAndStoredMessages);


            HashEntry[] NewConversationMessagesOfSingleUser = {

            new HashEntry(conversationGroupIdKey, convertingAllSingleConversationMessagesToStringToStoreInHash),

            };
            await redisDatabase.HashSetAsync("UsersAllMessagesDataStorage", NewConversationMessagesOfSingleUser);
        }

        // then delete it inside the redis as well when its completed one by one from recentlyMessages. => done
        private async Task DeleteSingleConversationRecentlyUsersMessageFromRedisHashStorage(string conversationGroupId, IDatabase redisDatabase)
        {
            await redisDatabase.HashDeleteAsync("RecentlyUsersMessagesStorage", conversationGroupId);
        }

    }
}
