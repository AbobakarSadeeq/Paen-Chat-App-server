using Business_Core.Entities;
using Business_Core.IServices;
using StackExchange.Redis;
using SpanJson;
using Hangfire;

namespace DataAccess.Services
{
    public class MessageRedisCacheService : IMessageRedisCacheService
    {
        private readonly IConnectionMultiplexer _redis;
        public MessageRedisCacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        private async Task CreateHashesInRedisAsync()
        {
            var redisDb = _redis.GetDatabase();
            if(await redisDb.HashLengthAsync("RecentlyUsersMessagesStorage") == 0) // hlen == O(1)
            {
                HashEntry[] insertDataToRedisHash = {
                   new HashEntry("CreateHash", "Default Value")
                };
                await redisDb.HashSetAsync("RecentlyUsersMessagesStorage", insertDataToRedisHash);
            }

            if (await redisDb.HashLengthAsync("UsersAllMessagesDataStorage") == 0) // hlen == O(1)
            {
                HashEntry[] insertDataToRedisHash = {
                   new HashEntry("CreateHash", "Default Value")
                };
                await redisDb.HashSetAsync("UsersAllMessagesDataStorage", insertDataToRedisHash);
            }

            if(redisDb.StringGet("ShiftingNewMessageDataTimeSpan").IsNull)
            {
                await redisDb.StringSetAsync("ShiftingNewMessageDataTimeSpan", DateTime.Today.ToString("d"));
            }
        }

      

        public async Task SaveMessageToHash(ClientMessageRedis clientMessage, string groupId)
        {
            await CreateHashesInRedisAsync();
            var redisDb = _redis.GetDatabase();
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
              //  await AddRecentlyMessagesHashToUsersAllMessagesStorageHashAsync();
                await redisDb.StringSetAsync("ShiftingNewMessageDataTimeSpan", assigningNewDate.ToString());

            }


        }

        public async Task GetSingleUserAllConnectedWithUsers()
        {
            var redisDb = _redis.GetDatabase();
            var getGroupIdData = await redisDb.HashGetAsync("ClientMessages", "GroupId4");
            //var stopWatch = new Stopwatch();
            var getList = await redisDb.HashGetAllAsync("ClientMessages");
            foreach (var item in getList)
            {
                Console.WriteLine(item.Name);
                Console.WriteLine(item.Value);
            }
            //stopWatch.Start();
            //var convertingToObject = ConvertingStringToObjects(getGroupIdData);
            //stopWatch.Stop();
            //Console.WriteLine("Simply way: " + stopWatch.ElapsedMilliseconds / 1000);

            // different way reading json
            //var stopWatch2 = new Stopwatch();
            //stopWatch2.Start();
            //var convertingObjectSecondWay = ConvertingStringToObjectsSecondWay(getGroupIdData);
            //while (convertingObjectSecondWay.Read())
            //{
            //    Console.WriteLine(convertingObjectSecondWay.Value);
            //}
            //stopWatch2.Stop();
            //Console.WriteLine("Second way: " + stopWatch2.ElapsedMilliseconds / 1000);



            string aa = "sad";
        }



        //private string ConvertingObjectToString(ClientMessageRedis singleMessage)
        //{
        //    var convertingObject = JsonSerializer.Generic.Utf16.Serialize(singleMessage);

        //    return convertingObject;
        //}

        private string ConvertingMultipleObjectsToString(List<ClientMessageRedis> usersMessagesList)
        {
            if (usersMessagesList == null)
                return string.Empty;

            var convertingObject = JsonSerializer.Generic.Utf16.Serialize(usersMessagesList);
            return convertingObject;
        }



        //private ClientMessageRedis ConvertingSingleStringObjectToObject(string singleMessage)
        //{
        //    if (singleMessage == null)
        //        return new ClientMessageRedis();

        //    var convertingString = JsonSerializer.Deserialize<ClientMessageRedis>(singleMessage);
        //    return convertingString;
        //}

        private List<ClientMessageRedis> ConvertingStringToObjects(string multipleMessages)
        {
            if (multipleMessages == null)
                return new List<ClientMessageRedis>();

            var convertingString = JsonSerializer.Generic.Utf16.Deserialize<List<ClientMessageRedis>>(multipleMessages);
            return convertingString;
        }

        public async Task SaveMessagesToDb()
        {
            // get all inside keys in hash for when storing data in hash
            var db = _redis.GetDatabase();
            var getAllKeys = await db.HashKeysAsync("RecentlyUsersMessagesStorage");
            // make repository for database
            // store all keys only value in database
            // use hangfire before 1 minute of remove data from redis
        }

        // cron
        // when 2 days past then add or messages to usersAllMessagesStorage hash where all users messages is stored
        // now to install the hangfire in this project and apply the timeing on in it
        // store that new message on db as well
        // store that in oldMessages as well in redis.
        // then delete it inside the redis as well when its completed.
        private async Task AddRecentlyMessagesHashToUsersAllMessagesStorageHashAsync()
        {
          
            // get the recently data first
            var selectingDb = _redis.GetDatabase();
            var getRecentlyAllUsersMessages = await selectingDb.HashKeysAsync("RecentlyUsersMessagesStorage");
            foreach (var singleConversationGroupKey in getRecentlyAllUsersMessages)
            {
                var getSingleConversationAllNewMessagesData = await selectingDb.HashGetAsync("RecentlyUsersMessagesStorage", singleConversationGroupKey);
                 await StoringAllSingleConversationMessagesToStorageHashRedisAsync(selectingDb, singleConversationGroupKey, getSingleConversationAllNewMessagesData);

            }
            // loop through on recently data

        }

        private async Task StoringAllSingleConversationMessagesToStorageHashRedisAsync(IDatabase redisDatabase, string conversationGroupIdKey, string allRecentMessagesOfSingleConversation)
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
    }
}
