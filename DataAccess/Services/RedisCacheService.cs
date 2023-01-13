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
    public interface IRedisCacheService
    {
       Task SaveMessageToHash(ClientSingleMessageViewModel viewModel, string groupId);

    }
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IConnectionMultiplexer _redis;
        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task SaveMessageToHash(ClientSingleMessageViewModel viewModel, string groupId)
        {
            // ClientMessages == hash name in cache
            // redis entry == inside that hash items
            var db = _redis.GetDatabase();
            var hashFoundInRedis = await db.HashLengthAsync("ClientMessages"); // hashLength is O(1)
            if(hashFoundInRedis == 0) 
            {
                HashEntry[] insertDataToRedisNewHash = {

                new HashEntry(groupId, ConvertingMultipleObjectsToString(new List<ClientSingleMessageViewModel>()
                {
                    viewModel
                }))

                };
                // create hash if not exist then it will be create as well and add the data you want to add.
                var setExpireTime = DateTime.Now.AddMinutes(2); // it will expire from now two minutes if that hash is not 

                await db.HashSetAsync("ClientMessages", insertDataToRedisNewHash);
                await db.KeyExpireAsync("ClientMessages", setExpireTime); // the hash will be expire when the time is up
                return;
            }

            // clientMessage hash is found inside the redis

            // fetching selected GroupId data in redis hash data structure
            var getGroupIdData = await db.HashGetAsync("ClientMessages", groupId);

            List<ClientSingleMessageViewModel> singleGroupMessage = ConvertingStringToObjects(getGroupIdData);
            singleGroupMessage.Add(viewModel);

            // again now serializing it for to store that new data in redis hash
            string newMessageAddedToHash = ConvertingMultipleObjectsToString(singleGroupMessage);

            HashEntry[] insertDataToRedisHash = {

            new HashEntry(groupId, newMessageAddedToHash),

            };
            await db.HashSetAsync("ClientMessages", insertDataToRedisHash);

        }



        private string ConvertingObjectToString(ClientSingleMessageViewModel singleMessage)
        {
            var convertingObject = JsonSerializer.Serialize(singleMessage);
            return convertingObject;
        }

        private string ConvertingMultipleObjectsToString(List<ClientSingleMessageViewModel> usersMessagesList)
        {
            if(usersMessagesList == null)
                return string.Empty;

            var convertingObject = JsonSerializer.Serialize(usersMessagesList);
            return convertingObject;
        }

        private ClientSingleMessageViewModel ConvertingSingleStringObjectToObject(string singleMessage)
        {
            if (singleMessage == null)
                return new ClientSingleMessageViewModel();

            var convertingString = JsonSerializer.Deserialize<ClientSingleMessageViewModel>(singleMessage);
            return convertingString;
        }

        private List<ClientSingleMessageViewModel> ConvertingStringToObjects(string multipleMessages)
        {
            if (multipleMessages == null)
                return new List<ClientSingleMessageViewModel>();

            var convertingString = JsonSerializer.Deserialize<List<ClientSingleMessageViewModel>>(multipleMessages);
            return convertingString;
        }
    }
}
