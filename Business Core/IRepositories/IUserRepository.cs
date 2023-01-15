using Business_Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Core.IRepositories
{
    public interface IUserRepository
    {
        Task RegisteringUserAsync(User user, string verificationNumber, string phoneNumber);
        Task<string> CheckingVerificationCodeAsync(string email, string entererdVerificationCode, string jwtSecreteKey);
        Task<object> AuthorizingUserAsync(string token);
        Task<object> FetchingUserProfileInfoAsync(int userId);
        Task AddingUserInfoAsync(AddUserInfo userInfo);
    }
}
