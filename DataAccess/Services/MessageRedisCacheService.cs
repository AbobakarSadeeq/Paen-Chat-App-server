using Business_Core.Entities;
using Business_Core.IServices;
using StackExchange.Redis;
using SpanJson;
using Presentation.ViewModel.Messages;
using Business_Core.Some_Data_Classes;
using Business_Core.FunctionParametersClasses;

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
                return await AddRecentlyMessagesHashToUsersAllMessagesStorageHashAsync(redisDb);

            }
            return new List<Message>();


        }

        // below algorithms thats called on upper methods
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
                    ReceiverId = clientMessageRedis.ReceiverId,
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

        // ---------------------------------------------- Fetching user messages from redis ----------------------------------------------
        public async Task<FetchingMessagesForUser> FetchingSingleConversationUsersMessagesFromRedis(SingleConversationMessagesParams funcParams)
        {
            var redisDb = _redis.GetDatabase();

            // recentlyUserMessagesRedis in redis
            // from client the fetchingMEssagesStorageNo will be change because if data
            if (funcParams.fetchingMessagesStorageNo == 1) // fetching data from recentlyUserMessageStorageRedis
            {
                var recentlyUserRedisMessagesStorageList = await GetMessagesFromRecentlyUserMessageStorageRedisAsync(redisDb, funcParams.groupId, funcParams.currentScrollMessangeNumber);
                if (recentlyUserRedisMessagesStorageList != null)
                {
                    var recentlyRedisStorageMessages = SwitchingBetweenRedisStoragesIfNeededAndDb(recentlyUserRedisMessagesStorageList, funcParams.fetchingMessagesStorageNo);
                    if (recentlyRedisStorageMessages.FetchedMessagesList.Count != 0)
                    {
                        return recentlyRedisStorageMessages;
                    }

                    funcParams.fetchingMessagesStorageNo = recentlyRedisStorageMessages.FetchingMessagesStorageNo; // it will become 2 here

                }




            }

            // userStorageMessageRedis in redis
            if (funcParams.fetchingMessagesStorageNo == 2)
            {
                var fetchingDataFromUserMessageStorageRedis = await GetMessagesFromUserMessagesStorageRedisAsync(redisDb, funcParams.groupId, funcParams.currentScrollMessangeNumber, funcParams.lastMessagesCount);

                if (fetchingDataFromUserMessageStorageRedis != null)
                {
                    var recentlyRedisStorageMessages = SwitchingBetweenRedisStoragesIfNeededAndDb(fetchingDataFromUserMessageStorageRedis, funcParams.fetchingMessagesStorageNo);
                    if (recentlyRedisStorageMessages.FetchedMessagesList.Count != 0)
                    {
                        return recentlyRedisStorageMessages;
                    }

                    funcParams.fetchingMessagesStorageNo = recentlyRedisStorageMessages.FetchingMessagesStorageNo; // it will become 3 here
                    return new FetchingMessagesForUser
                    {
                        FetchedMessagesList = new List<ClientMessageRedis>(),
                        FetchingMessagesStorageNo = 3 // now data base data return started and now db function need to have execute from controller.
                    };
                }
            }

            return  new FetchingMessagesForUser();
        }

        // ---------------------------------------------- Fetching user messages from Db ----------------------------------------------

        public async Task<FetchingMessagesForUser> FetchingSingleConversationUsersMessagesFromDb(SingleConversationMessagesParams funcParams, List<Message> dbMessages)
        {
            // fetching Data from db 

            var redisDb = _redis.GetDatabase();

            if (funcParams.fetchingMessagesStorageNo == 3)
            {

                await StroingSingleConversationAllMessagesOfDbOnUserMessageStorageRedisAsync(dbMessages, redisDb, funcParams.groupId); // CORRECT
                int skip = 0;
                var fetchingMessagesFromDbList = new List<Message>();
                if(funcParams.lastMessagesCount < 30)
                { 
                    if (funcParams.currentScrollMessangeNumber == 1)
                    {
                        fetchingMessagesFromDbList = dbMessages.Skip(funcParams.lastMessagesCount).Take(30 - funcParams.lastMessagesCount).ToList();
                    }
                    else
                    {
                        fetchingMessagesFromDbList = dbMessages.Skip((funcParams.currentScrollMessangeNumber - 1 * 30) + funcParams.lastMessagesCount).Take(30 - funcParams.lastMessagesCount).ToList();
                    }
                }else
                {
                    fetchingMessagesFromDbList = dbMessages.Skip(funcParams.currentScrollMessangeNumber - 1).Take(30).ToList();
                }
               

                // CORRECT

                var convertingMessageDbToRedisMessageFormate = ConvertingDbMessagesFormateIntoRedisStorageMessageFormate(fetchingMessagesFromDbList); // CORRECT

                convertingMessageDbToRedisMessageFormate.Reverse(); // CORRECT

                if (convertingMessageDbToRedisMessageFormate.Count < 30)  // CORRECT
                {
                    return new FetchingMessagesForUser // CORRECT
                    {
                        FetchedMessagesList = convertingMessageDbToRedisMessageFormate, // convert messages into ClientMessagesRedis tommarow
                        FetchingMessagesStorageNo = -1, // it means all data is completed and no data is found in messages storage to return prevs messages.
                    };
                }


                return new FetchingMessagesForUser // CORRECT
                {
                    FetchedMessagesList = convertingMessageDbToRedisMessageFormate, // convert messages into ClientMessagesRedis tommarow
                    FetchingMessagesStorageNo = 2, // if it completed 30 return then next messages fetching will be from redis again and not from db request again.
                };

            }

            return new FetchingMessagesForUser();

        }





        private async Task<List<ClientMessageRedis>> GetMessagesFromRecentlyUserMessageStorageRedisAsync(IDatabase redisDatabase, string groupId, int scrollCurrentNumber)
        {
            var findingMessagesOnRecentlyRedisStorageByGroupId = await redisDatabase.HashGetAsync("RecentlyUsersMessagesStorage", groupId);
            var deserializingJsonMessageStackObjects = ConvertingStringToStackObjects(findingMessagesOnRecentlyRedisStorageByGroupId);
            var fetchingMessagingStack = TakeAndSkipMessagesBasedOnScrollNumber(deserializingJsonMessageStackObjects, scrollCurrentNumber, 0);
            return fetchingMessagingStack;
        }

       

        private async Task<List<ClientMessageRedis>> GetMessagesFromUserMessagesStorageRedisAsync(IDatabase redisDatabase, string groupId, int scrollCurrentNumber, int lastMessagesCount)
        {
            var findingMessagesOnUsersRedisStorageMessagesByGroupId = await redisDatabase.HashGetAsync("UsersAllMessagesDataStorage", groupId);
            var deserializingJsonMessageStackObjects = ConvertingStringToStackObjects(findingMessagesOnUsersRedisStorageMessagesByGroupId);
            var fetchingMessagingStack = TakeAndSkipMessagesBasedOnScrollNumber(deserializingJsonMessageStackObjects, scrollCurrentNumber, lastMessagesCount);
            return fetchingMessagingStack;
        }


        private List<ClientMessageRedis> ConvertingStringToStackObjects(string multipleMessages)
        {
            if (multipleMessages == null)
                return new List<ClientMessageRedis>();

            var convertingStringToStackObjectsList = JsonSerializer.Generic.Utf16.Deserialize<List<ClientMessageRedis>>(multipleMessages);
            convertingStringToStackObjectsList.Reverse();
            return convertingStringToStackObjectsList;
        }


        private List<ClientMessageRedis> TakeAndSkipMessagesBasedOnScrollNumber(List<ClientMessageRedis> clientMessageStack, int currentNumberScroll, int lastMessagesCount)
        {
            if (currentNumberScroll == 1)
            {
                return clientMessageStack.Take(30 - lastMessagesCount).ToList(); // it will take last 30 because of stack
            }
            else
            {
                var takeAndSkipMessagesFromList = clientMessageStack.Skip((currentNumberScroll - 1) * 30).Take(30 - lastMessagesCount); // skip based on scroll and multiple with 30 and then take 30.
                return takeAndSkipMessagesFromList.ToList();

            }
        }

        private List<ClientMessageRedis> ConvertingDbMessagesFormateIntoRedisStorageMessageFormate(List<Message> singleConversationMessages)
        {
            List<ClientMessageRedis> redisMessages = new List<ClientMessageRedis>();
            foreach (var singleMessage in singleConversationMessages)
            {
                redisMessages.Add(new ClientMessageRedis
                {
                    UserMessage = singleMessage.UserMessage,
                    SenderId = singleMessage.SenderId,
                    ReceiverId = singleMessage.ReceiverId,
                    MessageSeen = singleMessage.MessageSeen,
                    MessageTimeStamp = singleMessage.Created_At.Value.TimeOfDay.ToString(),
                    MessageDateStamp = singleMessage.Created_At.Value.Date.ToString(),
                });
            }

            return redisMessages;
        }

        private async Task StroingSingleConversationAllMessagesOfDbOnUserMessageStorageRedisAsync(List<Message> allMessagesOfSingleConversationFromDb, IDatabase redisDb, string groupId)
        {
            var singleConversationAllMessagesRedisFormate = ConvertingDbMessagesFormateIntoRedisStorageMessageFormate(allMessagesOfSingleConversationFromDb);
            var convertingObjectsToString = ConvertingMultipleObjectsToString(singleConversationAllMessagesRedisFormate);
            HashEntry[] NewConversationMessagesOfSingleUser = {

                new HashEntry(groupId, convertingObjectsToString),

                 };
            await redisDb.HashSetAsync("UsersAllMessagesDataStorage", NewConversationMessagesOfSingleUser);
        }

        private FetchingMessagesForUser SwitchingBetweenRedisStoragesIfNeededAndDb(List<ClientMessageRedis> messageList, int fetchingMessagesStorageNo)
        {
            if (messageList.Count == 30)
            { // if full 30 completed then return it it all and dont do anything.
                return new FetchingMessagesForUser
                {
                    FetchedMessagesList = messageList,
                    FetchingMessagesStorageNo = fetchingMessagesStorageNo
                };
            }

            if (messageList.Count > 0)
            {
                // if less and not having more then return that all data then goto next storage, and again fetching then fetching it from other storage.
                return new FetchingMessagesForUser
                {
                    FetchedMessagesList = messageList,
                    FetchingMessagesStorageNo = fetchingMessagesStorageNo + 1
                };

            }


            // if completely 0 then goto next storage for fetching.
            return new FetchingMessagesForUser
            {
                FetchedMessagesList = messageList,
                FetchingMessagesStorageNo = fetchingMessagesStorageNo + 1,

            };
        }


    }
}
