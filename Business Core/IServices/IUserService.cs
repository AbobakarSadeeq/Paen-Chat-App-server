using Business_Core.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Core.IServices
{
    public interface IUserService
    {
        Task RegistrationUser(User user);
        Task<string> VerifyingVerificationCodeAsync(string email, string entererdVerificationCode);
        Task<object> AuthorizedUserInfoAsync(string token);
        Task<object> UserProfileInfoAsync(int userId);
        Task<AddUserInfo> AddUserProfileAsync(AddUserInfo userData);
    }
}
