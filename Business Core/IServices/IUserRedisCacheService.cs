using Business_Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Core.IServices
{
    public interface IUserRedisCacheService
    {
        Task UserAvailabilityStatusAddToSetRedis(string userId);
        Task UserAvailabilityStatusRemoveFromSetRedis(string userId);
        Task<bool> UserAvailabilityStatusChecking(string userId);
        Task StoringUserConnectedContactsGroupIdToRedisHashAsync(List<string> contactGroupIds, string userItSelfId);
        Task<List<string>> GetUserConnectedContactsGroupIdFromRedisHashInValidFormateAsync(string userItSelfId);
        Task RemoveUserConnectedContactsGroupIdFromRedisHashAsync(string userItSelfId);
        Task UserPhoneNumberVerificationSendCodeAsync(string phoneNumber);
        Task<bool> UserVerifyingSendedCodeAsync(string verificationCode, string phoneNumber);
        Task AddNewUserToRedisAsync(User user);


    }
}
