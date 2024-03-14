using Business_Core.Entities;
using Business_Core.IRepositories;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DataAccess.DataContext_Class;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Presentation.AppSettings;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _dataContext;
 
        public UserRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }



        public async Task RegisteringUserAsync(User user, string verificationNumber, string phoneNumber)
        {
           // var findingUserByEmail = await _dataContext.Users.Where(x => x.Email == user.Email).FirstOrDefaultAsync();
            //if (findingUserByEmail != null)
            //{
            //    findingUserByEmail.VerificationPassword = verificationNumber;
            //    _dataContext.Users.Update(findingUserByEmail);
            //} else
            //{
            //    // if email is not found then add the email there and generate the random verifcation password for it and contact number

            //    var addingUser = new User();
            //   // addingUser.Email = user.Email;
            //  //  addingUser.VerificationPassword = verificationNumber;
            //  //  addingUser.Created_At = DateTime.Now;
            //    addingUser.ContactNumber = phoneNumber;

            //    try
            //    {

            //        await _dataContext.AddAsync(addingUser);

            //    }
            //    catch (DbUpdateException ex) // if unique value is founded in contact number then update the contact value there.
            //    {
            //        addingUser.ContactNumber = addingUser.ContactNumber + addingUser.ContactNumber[addingUser.ContactNumber.Length - 1];
            //        await _dataContext.AddAsync(addingUser);
            //    }
            //}

        }

        public async Task<string> CheckingVerificationCodeAsync(string email, string entererdVerificationCode, string jwtSecreteKey)
        {
            //var validatingCurrentUserGeneratedCode = await _dataContext.Users.FirstOrDefaultAsync(a => a.Email == email);

            //    // checking the generate coded which is valid or not
            //    if (validatingCurrentUserGeneratedCode.VerificationPassword == entererdVerificationCode)
            //    {
            //        // user is validate means correct user
            //        // assigin to that user that code is used
            //        validatingCurrentUserGeneratedCode.VerificationPassword = "Code Used";
            //        validatingCurrentUserGeneratedCode.EmailVerification = true;
            //        _dataContext.Users.Update(validatingCurrentUserGeneratedCode);

            //        // generate a token
            //        // creatig jwt token
            //        List<Claim> myClaims = new List<Claim>
            //        {
            //            new Claim("UserId", validatingCurrentUserGeneratedCode.UserID.ToString()),
            //        };
            //        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSecreteKey));

            //        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //        var token = new JwtSecurityToken(
            //            claims: myClaims,
            //            expires: DateTime.Now.AddDays(7),
            //            signingCredentials: signingCredentials
            //            );

            //        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            //        return jwt;

            //    }
            //    else
            //    {
            //        return null;
            //    }
            return "";
        }

        public async Task<object> AuthorizingUserAsync(string token)
        {
            // decode the sended token
            var user = new User();
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token);
            var tokenS = jsonToken as JwtSecurityToken;
            var gettingDataFromTokenObj = tokenS?.Payload;
            foreach (var singleProp in gettingDataFromTokenObj)
            {
                if (singleProp.Key == "UserId")
                {
                    user = await _dataContext.Users
                        .FirstOrDefaultAsync(a => a.UserID == Convert.ToInt32(singleProp.Value));
                    break;
                }
            }

            return new
            {
                userId = user.UserID,
               // userName = user.UserName,
                userContactNumber = user.ContactNumber,
                userProfilePhoto = user.ProfilePhotoUrl,
              //  userStatus = user.About,
            };
        }

        public async Task<object> FetchingUserProfileInfoAsync(int userId)
        {
            var fetchingUserDataFromServer = await _dataContext.Users.FirstOrDefaultAsync(a => a.UserID == userId);
            return new
            {
              //  UserName = fetchingUserDataFromServer.UserName,
              //  AboutStatus = fetchingUserDataFromServer.About,
                ProfilePhotoUrl = fetchingUserDataFromServer.ProfilePhotoUrl,
              //  Email = fetchingUserDataFromServer.Email,
                Contact = fetchingUserDataFromServer.ContactNumber
            };
        }

        public async Task AddingUserInfoAsync(AddUserInfo userInfo)
        {
            var updatingUserProfileData = await _dataContext.Users.FirstOrDefaultAsync(a => a.UserID == userInfo.UserId);
         //   updatingUserProfileData.UserName = userInfo.UserName;
         //   updatingUserProfileData.About = userInfo.AboutStatus;
            if (userInfo.File != null)
            {
          //      updatingUserProfileData.PublicId = userInfo.PublicId;
          ///      updatingUserProfileData.ProfilePhotoUrl = userInfo.ProfilePhotoUrl;
            }
            _dataContext.Users.Update(updatingUserProfileData);

        }
    }
}
