using Business_Core.Entities;
using Business_Core.IServices;
using Business_Core.IUnitOfWork;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Presentation.AppSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;
        private readonly IConfiguration _configuration;


        public UserService(IUnitOfWork unitOfWork, IOptions<CloudinarySettings> cloudinaryConfig, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;


            _cloudinaryConfig = cloudinaryConfig;
            // Give the written keys that are in appsetting.json
            Account acc = new Account(
            _cloudinaryConfig.Value.CloudName,
            _cloudinaryConfig.Value.ApiKey,
            _cloudinaryConfig.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(acc);
        }

        #region RegistrationUser
        private string GeneratingVerificationNumber()
        {
            Random random = new Random();
            StringBuilder randomVerficationPasswordGenerateString = new StringBuilder();
            for (int i = 0; i < 5; i++)
            {
                randomVerficationPasswordGenerateString.Append(random.Next(1, 11));
            }
            return randomVerficationPasswordGenerateString.ToString();
        }

        private string GeneratingRandomUserPhoneNumber()
        {
            Random creatingRandomContactNumber = new Random();
            StringBuilder randomContactPasswordGenerateString = new StringBuilder();

            for (int i = 0; i < 6; i++)
            {
                randomContactPasswordGenerateString.Append(creatingRandomContactNumber.Next(0, 101));
            }

            while (randomContactPasswordGenerateString.Length > 11)
            {
                randomContactPasswordGenerateString.Remove(0, 1);
            }
            return randomContactPasswordGenerateString.ToString();
        }
        private void SendVerificationNumberEmail(string verificationNumber, string email)
        {
            MailMessage msgObj = new MailMessage("officalpaenchat@gmail.com", email);
            msgObj.Subject = "Paen chat verification code";
            msgObj.IsBodyHtml = true;
            msgObj.Body = $@"<h1>Paen chat verification code:</h1>
            <p>Your Paen chat verification code is <strong>{verificationNumber}.</strong></p> ";


            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.EnableSsl = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential() { UserName = "officalpaenchat@gmail.com", Password = "ogvlsqxesbwmhbrm" };
            client.Send(msgObj);
        }
        public async Task RegistrationUser(User user)
        {
            var verificationNumber = GeneratingVerificationNumber();
            var randomPhoneNumberGenrator = GeneratingRandomUserPhoneNumber();
            await _unitOfWork._userRepository.RegisteringUserAsync(user, verificationNumber, randomPhoneNumberGenrator);
           // SendVerificationNumberEmail(verificationNumber, user.Email);
            await _unitOfWork.CommitAsync();
        }

        #endregion

        public async Task<string> VerifyingVerificationCodeAsync(string email, string entererdVerificationCode)
        {
            var gettingTokenSecreteKey = _configuration.GetSection("ApplicationSettings:JWT_Secret").Value;
            var jwToken = await _unitOfWork._userRepository.CheckingVerificationCodeAsync(email, entererdVerificationCode, gettingTokenSecreteKey);
            return jwToken;
        }

        public async Task<object> AuthorizedUserInfoAsync(string token)
        {
            return await _unitOfWork._userRepository.AuthorizingUserAsync(token);
        }

        public async Task<object> UserProfileInfoAsync(int userId)
        {
            return await _unitOfWork._userRepository.FetchingUserProfileInfoAsync(userId);
        }

        public async Task<AddUserInfo> AddUserProfileAsync(AddUserInfo userData)
        {
           if(userData.File != null)
            {
                // store into cloudinary that image
                var uploadResult = new ImageUploadResult();

                using (var stream = userData.File.OpenReadStream())
                {
                    var uploadparams = new ImageUploadParams
                    {
                        File = new FileDescription(userData.File.Name, stream)
                    };
                    uploadResult = _cloudinary.Upload(uploadparams);
                }
                userData.PublicId = uploadResult.PublicId;
                userData.ProfilePhotoUrl = uploadResult.Url.ToString();
            }
           await _unitOfWork._userRepository.AddingUserInfoAsync(userData);
           await _unitOfWork.CommitAsync();

            return userData;
        }
    }
}
