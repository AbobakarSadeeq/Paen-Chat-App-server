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


            var currentTimeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            var fetchingMessagesShiftingFromNewToOldListDateFromRedis = await redisDb.StringGetAsync("ShiftingNewMessageDataTimeSpan");
            long convertingRedisSwitchingMessagesStringTimeStampToLong = long.Parse(fetchingMessagesShiftingFromNewToOldListDateFromRedis);
            if (convertingRedisSwitchingMessagesStringTimeStampToLong == currentTimeStamp || currentTimeStamp > convertingRedisSwitchingMessagesStringTimeStampToLong)
            {
                var futureTwoDaysTimeStamp = new DateTimeOffset(DateTime.UtcNow.AddDays(2)).ToUnixTimeSeconds();
                await redisDb.StringSetAsync("ShiftingNewMessageDataTimeSpan", futureTwoDaysTimeStamp); // because i did if again someone send request then dont again execute that condition.


                RedisValue[] listNames = await FetchingAllStoringNewMessagesConversationListNamesFromUniqueListInsideRedisAsync(redisDb);

                List<Message> usersAllMessagesList = new List<Message>();

                foreach (var singleListNameGroupId in listNames)
                {
                    string correctingGroupIdValue = singleListNameGroupId.ToString().Replace(":New", String.Empty);
                    var singleConversationAllMessagesList =  await StoringNewMessagesListIntoOldMessagesListInRedisAsync(correctingGroupIdValue, redisDb);
                    await DeleteNewConversationListAndItsNameInUniqueListFromRedisAsync(correctingGroupIdValue, redisDb);
                    usersAllMessagesList.AddRange(singleConversationAllMessagesList);
                }

               

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

        private List<Message> ConvertingSingleConversationAllMessagesToDbMessagesFormate(RedisValue [] singleListAllMessages)
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

        public Task<FetchingMessagesForUser> FetchingSingleConversationUsersMessagesFromRedis(SingleConversationMessagesParams funcParams)
        {
            throw new NotImplementedException();
        }

        public Task<FetchingMessagesForUser> FetchingSingleConversationUsersMessagesFromDb(SingleConversationMessagesParams funcParams, List<Message> dbMessages)
        {
            throw new NotImplementedException();
        }

        // old and abovve is new

        //private async Task CreateHashesAndAssignTimeSpanForUpdatingStoragesInRedisAsync(IDatabase redisDb)
        //{
        //    if(await redisDb.HashLengthAsync("RecentlyUsersMessagesStorage") == 0) // hlen == O(1)
        //    {
        //        HashEntry[] insertDataToRedisHash = {
        //           new HashEntry("CreateHash", "Default Value")
        //        };
        //        await redisDb.HashSetAsync("RecentlyUsersMessagesStorage", insertDataToRedisHash);
        //        await redisDb.HashDeleteAsync("RecentlyUsersMessagesStorage", "CreateHash"); // the hash will became empty now.
        //    }

        //    if (await redisDb.HashLengthAsync("UsersAllMessagesDataStorage") == 0) // hlen == O(1)
        //    {
        //        HashEntry[] insertDataToRedisHash = {
        //           new HashEntry("CreateHash", "Default Value")
        //        };
        //        await redisDb.HashSetAsync("UsersAllMessagesDataStorage", insertDataToRedisHash);
        //        await redisDb.HashDeleteAsync("UsersAllMessagesDataStorage", "CreateHash"); // the hash will became empty now.

        //    }

        //    if (redisDb.StringGet("ShiftingNewMessageDataTimeSpan").IsNull)
        //    {
        //        await redisDb.StringSetAsync("ShiftingNewMessageDataTimeSpan", DateTime.Today.ToString("d"));
        //    }
        //}

        //private List<ClientMessageRedis> ConvertingStringToObjects(string multipleMessages)
        //{
        //    if (multipleMessages == null)
        //        return new List<ClientMessageRedis>();

        //    var convertingString = JsonSerializer.Generic.Utf16.Deserialize<List<ClientMessageRedis>>(multipleMessages);
        //    return convertingString;
        //}
        //private string ConvertingMultipleObjectsToString(List<ClientMessageRedis> usersMessagesList)
        //{
        //    if (usersMessagesList == null)
        //        return string.Empty;

        //    var convertingObject = JsonSerializer.Generic.Utf16.Serialize(usersMessagesList);
        //    return convertingObject;
        //}

        //private async Task<List<Message>> AddRecentlyMessagesHashToUsersAllMessagesStorageHashAsync(IDatabase selectingDb)
        //{

        //    // get the recently data first
        //    List<Message> RecentlyNewAllConversationMessagesForStoreInDb = new List<Message>(); // => this list is going to send or store data on db and that will be sended to message service.

        //    var getRecentlyAllUsersMessages = await selectingDb.HashKeysAsync("RecentlyUsersMessagesStorage");
        //    foreach (var singleConversationGroupKey in getRecentlyAllUsersMessages)
        //    {

        //        var getSingleConversationAllNewMessagesData = await selectingDb.HashGetAsync("RecentlyUsersMessagesStorage", singleConversationGroupKey);

        //        List<Message> singleConversationNewMessages = ConvertRedisStoreSingleMessageIntoDbMessageFormate(getSingleConversationAllNewMessagesData);

        //        RecentlyNewAllConversationMessagesForStoreInDb.AddRange(singleConversationNewMessages);

        //         await StoringAllSingleConversationMessagesToHashRedisStorageAsync(selectingDb, singleConversationGroupKey, getSingleConversationAllNewMessagesData);

        //        await DeleteSingleConversationRecentlyUsersMessageFromRedisHashStorage(singleConversationGroupKey, selectingDb);
        //    }

        //    return RecentlyNewAllConversationMessagesForStoreInDb;
        //}
        //// store that new message on db as well => done
        //private List<Message> ConvertRedisStoreSingleMessageIntoDbMessageFormate(string singleConversationAllNewMessage)
        //{
        //    var convertStringIntoRedisMessageFormate = ConvertingStringToObjects(singleConversationAllNewMessage);

        //    List<Message> RecentlyNewMessagesOfSingleConversation = new List<Message>();

        //    foreach (var clientMessageRedis in convertStringIntoRedisMessageFormate)
        //    {
        //        Message singleMessage = new Message
        //        {
        //            SenderId = clientMessageRedis.SenderId,
        //            ReceiverId = clientMessageRedis.ReceiverId,
        //            UserMessage = clientMessageRedis.UserMessage,
        //            MessageSeen = clientMessageRedis.MessageSeen,
        //            Created_At = DateTime.Parse(clientMessageRedis.MessageDateStamp + " " + clientMessageRedis.MessageTimeStamp)
        //        };
        //        RecentlyNewMessagesOfSingleConversation.Add(singleMessage);
        //    }

        //    return RecentlyNewMessagesOfSingleConversation;
        //}

        //// store that in oldMessages as well in redis. => done
        //private async Task StoringAllSingleConversationMessagesToHashRedisStorageAsync(IDatabase redisDatabase, string conversationGroupIdKey, string allRecentMessagesOfSingleConversation)
        //{
        //    var fetchAllStoredMessagesOfSingleConversation = await redisDatabase.HashGetAsync("UsersAllMessagesDataStorage", conversationGroupIdKey);

        //    List<ClientMessageRedis> recentlySingleConversationAllMessages = ConvertingStringToObjects(allRecentMessagesOfSingleConversation);

        //    List<ClientMessageRedis> singleConversationUserAllRecentlyAndStoredMessages = ConvertingStringToObjects(fetchAllStoredMessagesOfSingleConversation);

        //    singleConversationUserAllRecentlyAndStoredMessages.AddRange(recentlySingleConversationAllMessages);

        //    string convertingAllSingleConversationMessagesToStringToStoreInHash = ConvertingMultipleObjectsToString(singleConversationUserAllRecentlyAndStoredMessages);


        //    HashEntry[] NewConversationMessagesOfSingleUser = {

        //    new HashEntry(conversationGroupIdKey, convertingAllSingleConversationMessagesToStringToStoreInHash),

        //    };
        //    await redisDatabase.HashSetAsync("UsersAllMessagesDataStorage", NewConversationMessagesOfSingleUser);
        //}

        //// then delete it inside the redis as well when its completed one by one from recentlyMessages. => done
        //private async Task DeleteSingleConversationRecentlyUsersMessageFromRedisHashStorage(string conversationGroupId, IDatabase redisDatabase)
        //{
        //    await redisDatabase.HashDeleteAsync("RecentlyUsersMessagesStorage", conversationGroupId);
        //}








        //// ---------------------------------------------- Fetching user messages from redis ----------------------------------------------
        //public async Task<FetchingMessagesForUser> FetchingSingleConversationUsersMessagesFromRedis(SingleConversationMessagesParams funcParams)
        //{
        //    var redisDb = _redis.GetDatabase();

        //    // recentlyUserMessagesRedis in redis
        //    // from client the fetchingMEssagesStorageNo will be change because if data
        //    if (funcParams.fetchingMessagesStorageNo == 1) // fetching data from recentlyUserMessageStorageRedis
        //    {
        //        var recentlyUserRedisMessagesStorageList = await GetMessagesFromRecentlyUserMessageStorageRedisAsync(redisDb, funcParams.groupId, funcParams.currentScrollMessangeNumber);
        //        if (recentlyUserRedisMessagesStorageList != null)
        //        {
        //            var recentlyRedisStorageMessages = SwitchingBetweenRedisStoragesIfNeededAndDb(recentlyUserRedisMessagesStorageList, funcParams.fetchingMessagesStorageNo);
        //            if (recentlyRedisStorageMessages.FetchedMessagesList.Count != 0)
        //            {
        //                recentlyRedisStorageMessages.LastMessagesCount = recentlyRedisStorageMessages.FetchedMessagesList.Count;
        //                return recentlyRedisStorageMessages;
        //            }
        //            // if no values found
        //            funcParams.fetchingMessagesStorageNo = recentlyRedisStorageMessages.FetchingMessagesStorageNo; // it will become 2 here

        //        }

        //        // messages count sended here of recentlyMessages and fetcheh only 


        //    }

        //    // userStorageMessageRedis in redis
        //    if (funcParams.fetchingMessagesStorageNo == 2)
        //    {
        //        var fetchingDataFromUserMessageStorageRedis = await GetMessagesFromUserMessagesStorageRedisAsync(redisDb, funcParams.groupId, funcParams.currentScrollMessangeNumber, funcParams.lastMessagesCount);

        //        if (fetchingDataFromUserMessageStorageRedis != null)
        //        {
        //            var recentlyRedisStorageMessages = SwitchingBetweenRedisStoragesIfNeededAndDb(fetchingDataFromUserMessageStorageRedis, funcParams.fetchingMessagesStorageNo);
        //            if (recentlyRedisStorageMessages.FetchedMessagesList.Count != 0)
        //            {
        //                return recentlyRedisStorageMessages;
        //            }

        //            funcParams.fetchingMessagesStorageNo = recentlyRedisStorageMessages.FetchingMessagesStorageNo; // it will become 3 here
        //            return new FetchingMessagesForUser
        //            {
        //                FetchedMessagesList = new List<ClientMessageRedis>(),
        //                FetchingMessagesStorageNo = 3 // now data base data return started and now db function need to have execute from controller.
        //            };
        //        }
        //    }

        //    return  new FetchingMessagesForUser();
        //}


        //// ---------------------------------------------- Fetching user messages from Db ----------------------------------------------

        //public async Task<FetchingMessagesForUser> FetchingSingleConversationUsersMessagesFromDb(SingleConversationMessagesParams funcParams, List<Message> dbMessages)
        //{
        //    // fetching Data from db 

        //    var redisDb = _redis.GetDatabase();

        //    if (funcParams.fetchingMessagesStorageNo == 3)
        //    {

        //        await StroingSingleConversationAllMessagesOfDbOnUserMessageStorageRedisAsync(dbMessages, redisDb, funcParams.groupId); // CORRECT']

        //        var fetchingMessagesFromDbList = new List<Message>();
        //        if(funcParams.lastMessagesCount < 30)
        //        { 
        //            if (funcParams.currentScrollMessangeNumber == 1)
        //            {
        //                fetchingMessagesFromDbList = dbMessages.Skip(funcParams.lastMessagesCount).Take(30 - funcParams.lastMessagesCount).ToList();
        //            }
        //            else
        //            { // if page is not currently 1 then this will be execute.
        //                fetchingMessagesFromDbList = dbMessages.Skip(((funcParams.currentScrollMessangeNumber - 1) * 30) + funcParams.lastMessagesCount).Take(30 - funcParams.lastMessagesCount).ToList();
        //            }
        //        }else
        //        { 
        //            fetchingMessagesFromDbList = dbMessages.Skip((funcParams.currentScrollMessangeNumber - 1) * 30).Take(30).ToList();
        //        }


        //        // CORRECT

        //        var convertingMessageDbToRedisMessageFormate = ConvertingDbMessagesFormateIntoRedisStorageMessageFormate(fetchingMessagesFromDbList); // CORRECT

        //        convertingMessageDbToRedisMessageFormate.Reverse(); // CORRECT

        //        if (convertingMessageDbToRedisMessageFormate.Count + funcParams.lastMessagesCount < 30)  // CORRECT
        //        {
        //            return new FetchingMessagesForUser // CORRECT
        //            {
        //                FetchedMessagesList = convertingMessageDbToRedisMessageFormate, // convert messages into ClientMessagesRedis tommarow
        //                FetchingMessagesStorageNo = -1, // it means all data is completed and no data is found in messages storage to return prevs messages.
        //            };
        //        }


        //        return new FetchingMessagesForUser // CORRECT
        //        {
        //            FetchedMessagesList = convertingMessageDbToRedisMessageFormate, // convert messages into ClientMessagesRedis tommarow
        //            FetchingMessagesStorageNo = 2, // if it completed 30 return then next messages fetching will be from redis again and not from db request again.
        //        };

        //    }

        //    return new FetchingMessagesForUser();

        //}

        //private async Task<List<ClientMessageRedis>> GetMessagesFromRecentlyUserMessageStorageRedisAsync(IDatabase redisDatabase, string groupId, int scrollCurrentNumber)
        //{
        //    var findingMessagesOnRecentlyRedisStorageByGroupId = await redisDatabase.HashGetAsync("RecentlyUsersMessagesStorage", groupId);
        //    var deserializingJsonMessageStackObjects = ConvertingStringToStackObjects(findingMessagesOnRecentlyRedisStorageByGroupId);
        //    var fetchingMessagingStack = TakeAndSkipMessagesBasedOnScrollNumber(deserializingJsonMessageStackObjects, scrollCurrentNumber, 0);
        //    return fetchingMessagingStack;
        //}

        //private async Task<List<ClientMessageRedis>> GetMessagesFromUserMessagesStorageRedisAsync(IDatabase redisDatabase, string groupId, int scrollCurrentNumber, int lastMessagesCount)
        //{
        //    var findingMessagesOnUsersRedisStorageMessagesByGroupId = await redisDatabase.HashGetAsync("UsersAllMessagesDataStorage", groupId);
        //    var deserializingJsonMessageStackObjects = ConvertingStringToStackObjects(findingMessagesOnUsersRedisStorageMessagesByGroupId);
        //    var fetchingMessagingStack = TakeAndSkipMessagesBasedOnScrollNumber(deserializingJsonMessageStackObjects, scrollCurrentNumber, lastMessagesCount);
        //    return fetchingMessagingStack;
        //}

        //private List<ClientMessageRedis> ConvertingStringToStackObjects(string multipleMessages)
        //{
        //    if (multipleMessages == null)
        //        return new List<ClientMessageRedis>();

        //    var convertingStringToStackObjectsList = JsonSerializer.Generic.Utf16.Deserialize<List<ClientMessageRedis>>(multipleMessages);
        //    convertingStringToStackObjectsList.Reverse();
        //    return convertingStringToStackObjectsList;
        //}

        //private List<ClientMessageRedis> TakeAndSkipMessagesBasedOnScrollNumber(List<ClientMessageRedis> clientMessageStack, int currentNumberScroll, int lastMessagesCount)
        //{
        //    if (currentNumberScroll == 1)
        //    {
        //        return clientMessageStack.Skip(lastMessagesCount).Take(30 - lastMessagesCount).ToList(); // it will take last 30 because of stack
        //    }
        //    else
        //    {
        //        var takeAndSkipMessagesFromList = clientMessageStack.Skip(((currentNumberScroll - 1) * 30 ) + lastMessagesCount).Take(30 - lastMessagesCount); // skip based on scroll and multiple with 30 and then take 30.
        //        return takeAndSkipMessagesFromList.ToList();

        //    }
        //}

        //private List<ClientMessageRedis> ConvertingDbMessagesFormateIntoRedisStorageMessageFormate(List<Message> singleConversationMessages)
        //{
        //    List<ClientMessageRedis> redisMessages = new List<ClientMessageRedis>();
        //    foreach (var singleMessage in singleConversationMessages)
        //    {
        //        redisMessages.Add(new ClientMessageRedis
        //        {
        //            UserMessage = singleMessage.UserMessage,
        //            SenderId = singleMessage.SenderId,
        //            ReceiverId = singleMessage.ReceiverId,
        //            MessageSeen = singleMessage.MessageSeen,
        //            MessageTimeStamp = singleMessage.Created_At.Value.TimeOfDay.ToString(),
        //            MessageDateStamp = singleMessage.Created_At.Value.Date.ToString(),
        //        });
        //    }

        //    return redisMessages;
        //}

        //private async Task StroingSingleConversationAllMessagesOfDbOnUserMessageStorageRedisAsync(List<Message> allMessagesOfSingleConversationFromDb, IDatabase redisDb, string groupId)
        //{
        //    var singleConversationAllMessagesRedisFormate = ConvertingDbMessagesFormateIntoRedisStorageMessageFormate(allMessagesOfSingleConversationFromDb);
        //    var convertingObjectsToString = ConvertingMultipleObjectsToString(singleConversationAllMessagesRedisFormate);
        //    HashEntry[] NewConversationMessagesOfSingleUser = {

        //        new HashEntry(groupId, convertingObjectsToString),

        //         };
        //    await redisDb.HashSetAsync("UsersAllMessagesDataStorage", NewConversationMessagesOfSingleUser);
        //}

        //private FetchingMessagesForUser SwitchingBetweenRedisStoragesIfNeededAndDb(List<ClientMessageRedis> messageList, int fetchingMessagesStorageNo)
        //{
        //    if (messageList.Count == 30)
        //    { // if full 30 completed then return it it all and dont do anything.
        //        return new FetchingMessagesForUser
        //        {
        //            FetchedMessagesList = messageList,
        //            FetchingMessagesStorageNo = fetchingMessagesStorageNo
        //        };
        //    }

        //    if (messageList.Count > 0)
        //    {
        //        // if less and not having more then return that all data then goto next storage, and again fetching then fetching it from other storage.
        //        return new FetchingMessagesForUser
        //        {
        //            FetchedMessagesList = messageList,
        //            FetchingMessagesStorageNo = fetchingMessagesStorageNo + 1
        //        };

        //    }


        //    // if completely 0 then goto next storage for fetching.
        //    return new FetchingMessagesForUser
        //    {
        //        FetchedMessagesList = messageList,
        //        FetchingMessagesStorageNo = fetchingMessagesStorageNo + 1,

        //    };
        //}

        // 

    }
}
