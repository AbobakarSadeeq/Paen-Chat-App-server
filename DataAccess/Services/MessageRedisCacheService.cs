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



        // ---------------------------------------------- Storing user messages On redis ----------------------------------------------



        public async Task<List<Message>> SaveMessagesInRedisAsync(ClientMessageRedis clientMessage, string groupId)
        {

            var redisDb = _redis.GetDatabase();

            var convertingObjectIntoSerializeObject = ConvertingSingleSendedMessageObjectIntoString(clientMessage);

            await StoringSingleNewMessageIntoNewConversationRedisListAsync(convertingObjectIntoSerializeObject, redisDb, groupId);

            await StoringNewMessagesConversationListNamesInUniqueListOfRedisInsideRedisAsync(groupId, redisDb);

            await AddingAndUpdatingSingleConversationRecentlyUsedTimeStampsInSortedSetsInRedisAsync(groupId);

            await AddingSixHoursTimeStampsForRemovingNotUsedRecentlyOldListFromRedisAsync();



            var currentTimeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            var fetchingMessagesShiftingFromNewToOldListDateFromRedis = await redisDb.StringGetAsync("ShiftingNewMessageDataTimeSpan");

            long convertingRedisSwitchingMessagesStringTimeStampToLong = 0;
            if (fetchingMessagesShiftingFromNewToOldListDateFromRedis.HasValue)
                convertingRedisSwitchingMessagesStringTimeStampToLong = long.Parse(fetchingMessagesShiftingFromNewToOldListDateFromRedis);


            if (convertingRedisSwitchingMessagesStringTimeStampToLong == currentTimeStamp || currentTimeStamp > convertingRedisSwitchingMessagesStringTimeStampToLong)
            {
                var futureTwoDaysTimeStamp = new DateTimeOffset(DateTime.UtcNow.AddDays(2)).ToUnixTimeSeconds();
                await redisDb.StringSetAsync("ShiftingNewMessageDataTimeSpan", futureTwoDaysTimeStamp); // because i did if again someone send request then dont again execute that condition.


                RedisValue[] listNames = await FetchingAllStoringNewMessagesConversationListNamesFromUniqueListInsideRedisAsync(redisDb);

                List<Message> usersAllMessagesList = new List<Message>();

                foreach (var singleListNameGroupId in listNames)
                {
                    string correctingGroupIdValue = singleListNameGroupId.ToString().Replace(":New", String.Empty);
                    var singleConversationAllMessagesList = await StoringNewMessagesListIntoOldMessagesListInRedisAsync(correctingGroupIdValue, redisDb);
                    await DeleteNewConversationListAndItsNameInUniqueListFromRedisAsync(correctingGroupIdValue, redisDb);
                    usersAllMessagesList.AddRange(singleConversationAllMessagesList);
                }

                // calling message service:
                return usersAllMessagesList;
            }


            return new List<Message>();

        }

        private string ConvertingSingleSendedMessageObjectIntoString(ClientMessageRedis singleMessage)
        {
            return JsonSerializer.Generic.Utf16.Serialize(singleMessage);
        }

        private async Task StoringSingleNewMessageIntoNewConversationRedisListAsync(string singleSerializeMessage, IDatabase redisDb, string groupId)
        {
            // first have to create list in redis with the groupId:New
            await redisDb.ListLeftPushAsync($"{groupId}:New", singleSerializeMessage);
        }

        private async Task StoringNewMessagesConversationListNamesInUniqueListOfRedisInsideRedisAsync(string groupId, IDatabase redisDb)
        {
            // i dont have to search about that is that value is there or not becuase it will not give me exception
            await redisDb.SetAddAsync("NewConversationListNames", $"{groupId}:New");
        }

        private async Task<List<Message>> StoringNewMessagesListIntoOldMessagesListInRedisAsync(string groupId, IDatabase redisDb)
        {
            var sourceNewRedisList = await redisDb.ListRangeAsync($"{groupId}:New", 0, -1);
            Array.Reverse(sourceNewRedisList);
            await redisDb.ListLeftPushAsync($"{groupId}:Old", sourceNewRedisList);
            return ConvertingSingleConversationAllMessagesToDbMessagesFormate(sourceNewRedisList);



        }
        private async Task DeleteNewConversationListAndItsNameInUniqueListFromRedisAsync(string groupId, IDatabase redisDb)
        {
            await redisDb.KeyDeleteAsync($"{groupId}:New");
            await redisDb.SetRemoveAsync("NewConversationListNames", $"{groupId}:New");
        }

        private async Task<RedisValue[]> FetchingAllStoringNewMessagesConversationListNamesFromUniqueListInsideRedisAsync(IDatabase redisdatabase)
        {
            var listNamesOfNewConversationList = await redisdatabase.SetMembersAsync("NewConversationListNames");
            return listNamesOfNewConversationList;


        }

        private List<Message> ConvertingSingleConversationAllMessagesToDbMessagesFormate(RedisValue[] singleListAllMessages)
        {
            List<Message> messages = new List<Message>();
            foreach (var message in singleListAllMessages)
            {
                var singleMessageConversation = JsonSerializer.Generic.Utf16.Deserialize<ClientMessageRedis>(message);

                messages.Add(new Message
                {
                    SenderId = singleMessageConversation.SenderId,
                    ReceiverId = singleMessageConversation.ReceiverId,
                    MessageSeen = singleMessageConversation.MessageSeen,
                    Created_At = DateTime.Parse(singleMessageConversation.MessageDateStamp + " " + singleMessageConversation.MessageTimeStamp),
                    UserMessage = singleMessageConversation.UserMessage,
                });
            }
            return messages;
        }



        // --------------------------- LRU --------------------------------------



        private async Task AddingAndUpdatingSingleConversationRecentlyUsedTimeStampsInSortedSetsInRedisAsync(string groupId)
        {
            var redisDb = _redis.GetDatabase();
            var currentTimeStampPlusAddTwoDays = new DateTimeOffset(DateTime.UtcNow.AddDays(2)).ToUnixTimeSeconds();
            await redisDb.SortedSetAddAsync("LeastRecentlyUsedConversationList", groupId, currentTimeStampPlusAddTwoDays);

        }

        private async Task AddingSixHoursTimeStampsForRemovingNotUsedRecentlyOldListFromRedisAsync()
        {
            var redisDb = _redis.GetDatabase();
            var currentTimeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            var fetchingMessagesShiftingFromNewToOldListDateFromRedis = await redisDb.StringGetAsync("LeastRecentlyUsedConversationDataTimeSpan");
            long convertingRedisRecentlyUsedStringTimeStampToLong = 0;
            if (fetchingMessagesShiftingFromNewToOldListDateFromRedis.HasValue)
                 convertingRedisRecentlyUsedStringTimeStampToLong = long.Parse(fetchingMessagesShiftingFromNewToOldListDateFromRedis);


            if (convertingRedisRecentlyUsedStringTimeStampToLong == currentTimeStamp || currentTimeStamp > convertingRedisRecentlyUsedStringTimeStampToLong)
            {
                await RemovingNotUsedRecentlyOldListFromRedisAsync();
                var futureSixHoursTimeStamp = new DateTimeOffset(DateTime.UtcNow.AddHours(6)).ToUnixTimeSeconds();
                await redisDb.StringSetAsync("LeastRecentlyUsedConversationDataTimeSpan", futureSixHoursTimeStamp);
            }

        }

        private async Task  RemovingNotUsedRecentlyOldListFromRedisAsync()
        {
            var redisDb = _redis.GetDatabase();
            var currentTimeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            var beforeTwoDays = currentTimeStamp - 172800; 
            var leastRecentlyUsedConversationList = await redisDb.SortedSetRangeByScoreAsync("LeastRecentlyUsedConversationList", 0, beforeTwoDays);

            foreach (var singleLRU in leastRecentlyUsedConversationList)
            {
                await redisDb.KeyDeleteAsync($"{singleLRU.ToString()}:Old");
                await redisDb.SortedSetRemoveAsync("LeastRecentlyUsedConversationList", singleLRU.ToString());

            }
            // fetching from today current time and date to last two days.
        }



        //// ---------------------------------------------- Fetching user messages from redis ----------------------------------------------


        public async Task<FetchingMessagesForUser> FetchingSingleConversationUsersMessagesFromRedisAsync(SingleConversationMessagesParams funcParams)
        {
            var redisDb = _redis.GetDatabase();


            if (funcParams.fetchingMessagesStorageNo == 1) // fetching data from user:NewList
            {
                var fetchingMessagesFromNewListMessagesRedis = await GetSingleConversationMessagesFromUserNewOrOldList(redisDb, funcParams.groupId, funcParams.currentScrollingPosition, "New", funcParams.lastMessagesCount);
                if (fetchingMessagesFromNewListMessagesRedis != null)
                {
                    var recentlyRedisStorageMessages = SwitchingBetweenRedisStoragesIfNeededAndDb(fetchingMessagesFromNewListMessagesRedis, funcParams.fetchingMessagesStorageNo, funcParams.lastMessagesCount);

                    if (recentlyRedisStorageMessages.FetchedMessagesList.Count != 0)
                    {
                        // if data is greater then 0 then this will execute even 30 then and if less then 30 then return fetch from next and count and list
                        if (recentlyRedisStorageMessages.FetchedMessagesList.Count == 30)
                            recentlyRedisStorageMessages.LastMessagesCount = 0;
                        else
                            recentlyRedisStorageMessages.LastMessagesCount = recentlyRedisStorageMessages.FetchedMessagesList.Count;
                    
                        return recentlyRedisStorageMessages;
                    } // if zero and no value next found then this will be execute.
                    funcParams.fetchingMessagesStorageNo = recentlyRedisStorageMessages.FetchingMessagesStorageNo; // it will become 2 here
                }
            }

            if (funcParams.fetchingMessagesStorageNo == 2)
            {
                var fetchingMessagesFromNewListMessagesRedis = await GetSingleConversationMessagesFromUserNewOrOldList(redisDb, funcParams.groupId, funcParams.currentScrollingPosition, "Old", funcParams.lastMessagesCount);

                if (fetchingMessagesFromNewListMessagesRedis != null)
                {
                    var recentlyRedisStorageMessages = SwitchingBetweenRedisStoragesIfNeededAndDb(fetchingMessagesFromNewListMessagesRedis, funcParams.fetchingMessagesStorageNo, funcParams.lastMessagesCount);

                    if (recentlyRedisStorageMessages.FetchedMessagesList.Count != 0)
                    {
                        if (recentlyRedisStorageMessages.FetchedMessagesList.Count == 30 || recentlyRedisStorageMessages.FetchedMessagesList.Count > 30)
                            recentlyRedisStorageMessages.LastMessagesCount = 0;
                        else
                            recentlyRedisStorageMessages.LastMessagesCount = recentlyRedisStorageMessages.FetchedMessagesList.Count;

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

             return new FetchingMessagesForUser();
        }


        private async Task<List<ClientMessageRedis>> GetSingleConversationMessagesFromUserNewOrOldList(IDatabase redisDb, string groupId, int scrollingPosition, string fromListName,int lastMessageCount)
        {
            if(scrollingPosition == 1)
            {
                if(lastMessageCount > 0) // this will execute when new list does not having the data or less then 30 then it will execute on old list 
                    // when fetching data then 30 is fetching from redis db like 0 start and end will be 29 which means 29 
                {
                    var fetchingLast30MessagesFromRedis = await redisDb.ListRangeAsync($"{groupId}:{fromListName}",0, -1);  // all messages
                    var takeAndSkipingList = fetchingLast30MessagesFromRedis.Take(30 - lastMessageCount).ToList();
                    var newListMessagess = await ConvertingMessagesToObjects(takeAndSkipingList);
                    return newListMessagess;
                }

                var fetchingLast30MessagesFromRediss = await redisDb.ListRangeAsync($"{groupId}:{fromListName}", 0, -1);  // all messages
                var takeAndSkipList = fetchingLast30MessagesFromRediss.Take(30).ToList();
                var newListMessages = await ConvertingMessagesToObjects(takeAndSkipList);
                return newListMessages;
            }else
            {  
                // 2
                var fetchingLast30MessagesFromRedis = await redisDb
                    .ListRangeAsync($"{groupId}:{fromListName}", 0, -1);
                int startingFrom = ((scrollingPosition - 1) * 30) - lastMessageCount;
                List<RedisValue> takeAndSkipingList = new List<RedisValue>();
                if (lastMessageCount < 30 && lastMessageCount > 0)
                takeAndSkipingList = fetchingLast30MessagesFromRedis.Skip(lastMessageCount).Take(30 + startingFrom).ToList();
                else
                   takeAndSkipingList = fetchingLast30MessagesFromRedis.Skip(startingFrom).Take(30).ToList();



                var newListMessages = await ConvertingMessagesToObjects(takeAndSkipingList);
                return newListMessages;
            }


        }

        private Task<List<ClientMessageRedis>> ConvertingMessagesToObjects(List<RedisValue> messages) 
        {
            List<ClientMessageRedis> result = new List<ClientMessageRedis>();
            foreach (var singleMessage in messages)
            {
            var messagesList = JsonSerializer.Generic.Utf16.Deserialize<ClientMessageRedis>(singleMessage);
                result.Add(messagesList);
            }
            return Task.FromResult(result);

        }






        //// ---------------------------------------------- Fetching user messages from Db ----------------------------------------------

        public async Task<FetchingMessagesForUser> FetchingSingleConversationUsersMessagesFromDb(SingleConversationMessagesParams funcParams, List<Message> dbMessages)
        {
            // fetching Data from db 

            var redisDb = _redis.GetDatabase();

            if (funcParams.fetchingMessagesStorageNo == 3)
            {
                var fetchingMessagesFromDbList = new List<Message>();
                if (funcParams.lastMessagesCount < 30)
                {
                    if (funcParams.currentScrollingPosition == 1)
                    {
                        fetchingMessagesFromDbList = dbMessages.Skip(funcParams.lastMessagesCount ).Take(30 - funcParams.lastMessagesCount).ToList();
                    }
                    else
                    { // if page is not currently 1 then this will be execute.
                        fetchingMessagesFromDbList = dbMessages.Skip(((funcParams.currentScrollingPosition - 1) * 30) + funcParams.lastMessagesCount).Take(30 - funcParams.lastMessagesCount).ToList();
                    }
                }
                else
                {
                    fetchingMessagesFromDbList = dbMessages.Skip((funcParams.currentScrollingPosition - 1) * 30).Take(30).ToList();
                }

 

                var convertingMessageDbToRedisMessageFormate = ConvertingReturningMessagesFromDbIntoRedisFormate(fetchingMessagesFromDbList);

                convertingMessageDbToRedisMessageFormate.Reverse();

                if (convertingMessageDbToRedisMessageFormate.Count + funcParams.lastMessagesCount < 30)  // CORRECT
                {
                    return new FetchingMessagesForUser // CORRECT
                    {
                        FetchedMessagesList = convertingMessageDbToRedisMessageFormate, // convert messages into ClientMessagesRedis tommarow
                        FetchingMessagesStorageNo = -1, // it means all data is completed and no data is found in messages storage to return prevs messages.

                    };
                }

                await StroingSingleConversationAllMessagesFromDbOnOldUserMessagesListRedisAsync(dbMessages, redisDb, funcParams.groupId);

                return new FetchingMessagesForUser // CORRECT
                {
                    FetchedMessagesList = convertingMessageDbToRedisMessageFormate, // convert messages into ClientMessagesRedis tommarow
                    FetchingMessagesStorageNo = 2, // if it completed 30 return then next messages fetching will be from redis again and not from db request again.
                };

            }
                return new FetchingMessagesForUser();

        }



        private async Task StroingSingleConversationAllMessagesFromDbOnOldUserMessagesListRedisAsync(List<Message> dbMessages, IDatabase redisDb, string groupId)
        {
            await redisDb.KeyDeleteAsync($"{groupId}:Old");
            await ConvertingDbMessagesFormateIntoRedisStorageMessageFormateAndStoringMessagesToRedisAsync(dbMessages, redisDb,groupId);
        }

        private async Task ConvertingDbMessagesFormateIntoRedisStorageMessageFormateAndStoringMessagesToRedisAsync(List<Message> singleConversationMessages, IDatabase redisDb, string groupId)
        {
            foreach (var singleMessage in singleConversationMessages)
            {
                var formetedClientMessage = new ClientMessageRedis
                {
                    UserMessage = singleMessage.UserMessage,
                    SenderId = singleMessage.SenderId,
                    ReceiverId = singleMessage.ReceiverId,
                    MessageSeen = singleMessage.MessageSeen,
                    MessageTimeStamp = singleMessage.Created_At.Value.TimeOfDay.ToString(),
                    MessageDateStamp = singleMessage.Created_At.Value.Date.ToString(),
                };

                string singleDbMessageJson = JsonSerializer.Generic.Utf16.Serialize(formetedClientMessage);
                await redisDb.ListLeftPushAsync($"{groupId}:Old", singleDbMessageJson);

            }



        }

        private  List<ClientMessageRedis> ConvertingReturningMessagesFromDbIntoRedisFormate(List<Message> messages)
        {
            List<ClientMessageRedis> returningMessages = new List<ClientMessageRedis>();
            foreach (var singleMessage in messages)
            {
                var formetedClientMessage = new ClientMessageRedis
                {
                    UserMessage = singleMessage.UserMessage,
                    SenderId = singleMessage.SenderId,
                    ReceiverId = singleMessage.ReceiverId,
                    MessageSeen = singleMessage.MessageSeen,
                    MessageTimeStamp = singleMessage.Created_At.Value.TimeOfDay.ToString(),
                    MessageDateStamp = singleMessage.Created_At.Value.Date.ToString(),
                };
                returningMessages.Add(formetedClientMessage);
            }

            return returningMessages;
        }
      

        private FetchingMessagesForUser SwitchingBetweenRedisStoragesIfNeededAndDb(List<ClientMessageRedis> messageList, int fetchingMessagesStorageNo, int lastMessageCount)
        {
            if (messageList.Count + lastMessageCount == 30 || messageList.Count + lastMessageCount > 30)
            { // if full 30 completed then return it it all and dont do anything.
                return new FetchingMessagesForUser
                {
                    FetchedMessagesList = messageList,
                    FetchingMessagesStorageNo = fetchingMessagesStorageNo
                };
            }


            if (messageList.Count + lastMessageCount > 0 && messageList.Count + lastMessageCount < 30)
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
