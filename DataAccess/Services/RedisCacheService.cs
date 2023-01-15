using Business_Core.Entities;
using Business_Core.IServices;
using Microsoft.Extensions.Caching.Distributed;
using Presentation.ViewModel.Messages;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DataAccess.Services
{

    public class RedisCacheService : IRedisCacheService
    {
        private readonly IConnectionMultiplexer _redis;
        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task SaveMessageToHash(ClientMessageRedis clientMessage, string groupId)
        {
            


            // ClientMessages == hash name in cache
            // redis entry == inside that hash items
            var db = _redis.GetDatabase();

            var hashFoundInRedis = await db.HashLengthAsync("ClientMessages"); // hashLength is O(1)
            if(hashFoundInRedis == 0) 
            {
                HashEntry[] insertDataToRedisNewHash = {

                new HashEntry(groupId, ConvertingMultipleObjectsToString(new List<ClientMessageRedis>()
                {
                    clientMessage
                }))

                };
                // create hash if not exist then it will be create as well and add the data you want to add.
                var setExpireTime = DateTime.Now.AddMinutes(2); // it will expire from now two minutes if that hash is not 

                await db.HashSetAsync("ClientMessages", insertDataToRedisNewHash);
              //  await db.KeyExpireAsync("ClientMessages", setExpireTime); // the hash will be expire when the time is up
                
                return;
            }

            // clientMessage hash is found inside the redis

            // fetching selected GroupId data in redis hash data structure
            var getGroupIdData = await db.HashGetAsync("ClientMessages", groupId);

            List<ClientMessageRedis> singleGroupMessage = ConvertingStringToObjects(getGroupIdData);
            singleGroupMessage.Add(clientMessage);

            // again now serializing it for to store that new data in redis hash
            string newMessageAddedToHash = ConvertingMultipleObjectsToString(singleGroupMessage);

            HashEntry[] insertDataToRedisHash = {

            new HashEntry(groupId, newMessageAddedToHash),

            };
            await db.HashSetAsync("ClientMessages", insertDataToRedisHash);

        }



        private string ConvertingObjectToString(ClientMessageRedis singleMessage)
        {
            var convertingObject = JsonSerializer.Serialize(singleMessage);
            return convertingObject;
        }

        private string ConvertingMultipleObjectsToString(List<ClientMessageRedis> usersMessagesList)
        {
            if(usersMessagesList == null)
                return string.Empty;

            var convertingObject = JsonSerializer.Serialize(usersMessagesList);
            return convertingObject;
        }

        private ClientMessageRedis ConvertingSingleStringObjectToObject(string singleMessage)
        {
            if (singleMessage == null)
                return new ClientMessageRedis();

            var convertingString = JsonSerializer.Deserialize<ClientMessageRedis>(singleMessage);
            return convertingString;
        }

        private List<ClientMessageRedis> ConvertingStringToObjects(string multipleMessages)
        {
            if (multipleMessages == null)
                return new List<ClientMessageRedis>();

            var convertingString = JsonSerializer.Deserialize<List<ClientMessageRedis>>(multipleMessages);
            return convertingString;
        }

        public async Task SaveMessagesToDb()
        {
            // get all inside keys in hash for when storing data in hash
            var db = _redis.GetDatabase();
            var getAllKeys = await db.HashKeysAsync("ClientMessages");
            // make repository for database
            // store all keys only value in database
            // use hangfire before 1 minute of remove data from redis
        }
    }
}
