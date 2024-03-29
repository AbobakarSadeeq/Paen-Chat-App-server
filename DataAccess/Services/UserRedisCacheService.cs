﻿using Business_Core.IServices;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpanJson;
using Business_Core.Entities;

namespace DataAccess.Services
{
    public class UserRedisCacheService : IUserRedisCacheService
    {
        private readonly IConnectionMultiplexer _redis;
        public UserRedisCacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }
        public async Task UserAvailabilityStatusAddToSetRedis(string userId)
        {
            var redisDb = _redis.GetDatabase();
            if (await redisDb.SetContainsAsync("UsersAvailabilityStatus", userId.ToString()) == false)
                await redisDb.SetAddAsync("UsersAvailabilityStatus", userId.ToString());

        }

        public async Task UserAvailabilityStatusRemoveFromSetRedis(string userId)
        {
            var redisDb = _redis.GetDatabase();
            await redisDb.SetRemoveAsync("UsersAvailabilityStatus", userId.ToString());
        }

        public async Task<bool> UserAvailabilityStatusChecking(string userId)
        {
            var redisDb = _redis.GetDatabase();
            if (await redisDb.SetContainsAsync("UsersAvailabilityStatus", userId.ToString()))
                return true;
            else
                return false;

        }
        // ----------------------------- New Code -----------------------------
        public async Task UserPhoneNumberVerificationSendCodeAsync(string phoneNumber)
        {
            var redisDb = _redis.GetDatabase();
            string verificationNumberGenerate = RandomGenerateSixDigitsVerificationCode();
            await redisDb.HashSetAsync("UserVerificationCodes", new HashEntry[]
            {
                new HashEntry(phoneNumber, verificationNumberGenerate)
            });
        }

        private string RandomGenerateSixDigitsVerificationCode()
        {
            Random random = new Random();
            StringBuilder randomVerficationPasswordGenerateString = new StringBuilder();
            for (int i = 0; i < 6; i++)
            {
                randomVerficationPasswordGenerateString.Append(random.Next(1, 9));
            }
            return randomVerficationPasswordGenerateString.ToString();
        }

        public async Task<bool> UserVerifyingSendedCodeAsync(string verificationCode, string phoneNumber)
        {
            var redisDb = _redis.GetDatabase();
            var getVerificationCodeByPhoneNumber = await redisDb.HashGetAsync("UserVerificationCodes", phoneNumber);
            if (verificationCode != getVerificationCodeByPhoneNumber.ToString())
                return false;
            await redisDb.HashDeleteAsync("UserVerificationCodes", phoneNumber);

            return true;

        }

        public async Task AddNewUserToRedisAsync(User user)
        {
            // use the redis hash
            // key = phoneNumner, value = userInfo
            var redisDb = _redis.GetDatabase();
            if (await redisDb.HashGetAsync("Users", user.ContactNumber) != true)
            {
                // add new user to the redis if a that specefic phoneNumber does found in redis
                string jsonStringObj = convertUserObjectIntoJson(user);
                await redisDb.HashSetAsync("Users", new HashEntry[]
                {
                new HashEntry(user.ContactNumber, jsonStringObj)
                });
            }

        }

        private string convertUserObjectIntoJson(User user)
        {
            var mappingUser = new
            {
                aboutStatus = user.AboutStatus,
                contactNumber = user.ContactNumber,
                fullName = user.FullName,
                profilePhotoUrl = user.ProfilePhotoUrl,
                UserGroupPrivateConnectionIds = "",
                createdAt = DateTime.Now,
                LastOnline = ""
            };
            return JsonSerializer.Generic.Utf16.Serialize(mappingUser);
        }

        // ----------------------------- End New Code -----------------------------


        // Redis Hash data 
        public async Task StoringUserConnectedContactsGroupIdToRedisHashAsync(List<string> contactGroupIds, string userItSelfId)
        {
            var redisDb = _redis.GetDatabase();
            string verifiedContactGroupIdsInString = JsonSerializer.Generic.Utf16.Serialize(contactGroupIds);
            await redisDb.HashSetAsync("UsersVerifiedContactsGroupIds", new HashEntry[]
            {
                new HashEntry(userItSelfId, verifiedContactGroupIdsInString)
            });
        }

        public async Task RemoveUserConnectedContactsGroupIdFromRedisHashAsync(string userItSelfId)
        {
            var redisDb = _redis.GetDatabase();
            await redisDb.HashDeleteAsync("UsersVerifiedContactsGroupIds", new RedisValue[] { userItSelfId });

        }

        public async Task<List<string>> GetUserConnectedContactsGroupIdFromRedisHashInValidFormateAsync(string userItSelfId)
        {
            var redisDb = _redis.GetDatabase();
            RedisValue redisHashSingleElement = await redisDb.HashGetAsync("UsersVerifiedContactsGroupIds", userItSelfId);
            string value = redisHashSingleElement.ToString();
            List<string> verifiedContactGroupIdsInString = JsonSerializer.Generic.Utf16.Deserialize<List<string>>(value);
            return verifiedContactGroupIdsInString;

        }


    }
}
