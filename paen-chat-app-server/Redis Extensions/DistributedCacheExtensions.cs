using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace paen_chat_app_server.Redis_Extensions
{
    public static class DistributedCacheExtensions
    {
        public static async Task SetValueToRedisAsync<T>(this IDistributedCache cache, 
            string  recordId, // key or unique identifier for this cache item
            T data, // what ever we want to store we can store or what ever data-type is.
            TimeSpan? absoluteExpireTime = null, // expire time or removing data time here assigning and dont assign large expires time assign only 1 day or hours only
            TimeSpan? unusedExpireTime = null) // expire time of that data whose not in used or called then redis will auto delete that data after your unused data expire time and its optional
        {
            var options = new DistributedCacheEntryOptions(); // option are here means how much that data or set is going to stay here or other option.


            options.AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromMinutes(60); // this line is used for when want to full remove data from redis cache then we assgin the time here after that much time remove it
            options.SlidingExpiration = unusedExpireTime; // if you didnt used the cache item for the given time period then it will automaticaly expired it means used to data time will be define here.

            var jsonData = JsonSerializer.Serialize(data); // here we converting any time data to bytes so, it can store easily and not ocuppy alot space
            await cache.SetStringAsync(recordId, jsonData, options); // here we are assign value to the redis which is in key value pair form like variableName = value and saving data to redis
        
        
        }

        public static async Task<T> GetRecordAsync<T>(this IDistributedCache cache,
            string recordId)
        {
            var jsonData = await cache.GetStringAsync(recordId);
            // if key is not found inside the redis cache
            if(jsonData == null)
            {
                return default(T);
            }

            return JsonSerializer.Deserialize<T>(jsonData);
        }
    }
}
