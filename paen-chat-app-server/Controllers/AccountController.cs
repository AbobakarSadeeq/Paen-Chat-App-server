using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DataAccess.DataContext_Class;
using DataAccess.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Presentation.AppSettings;
using Presentation.ViewModel;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace paen_chat_app_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private DataContext _dataContext;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;
        private readonly IConfiguration configuration;

        public AccountController(DataContext dataContext, IConfiguration configuration,
                  IOptions<CloudinarySettings> cloudinaryConfig)
        {
            this.configuration = configuration;
            _dataContext = dataContext;
            _cloudinaryConfig = cloudinaryConfig;
            // Give the written keys that are in appsetting.json
            Account acc = new Account(
            _cloudinaryConfig.Value.CloudName,
            _cloudinaryConfig.Value.ApiKey,
            _cloudinaryConfig.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(acc);
        }

        [HttpPost]
        // this algo having add new user and assign random generate contact-number to database and generate a verfication code and store in database and send verfication code to gmail.
        public async Task<IActionResult> RegisteringUser(UserLogInViewModel viewModel)
        {
            int generatedUserId = 0;
            Random random = new Random();
            StringBuilder randomVerficationPasswordGenerateString = new StringBuilder();
            for (int i = 0; i < 5; i++)
            {
                randomVerficationPasswordGenerateString.Append(random.Next(1, 11));
            }

            // converting password to hash

            var findingUserByEmail = await _dataContext.Users.Where(x => x.Email == viewModel.Email).FirstOrDefaultAsync();
            if (findingUserByEmail != null)
            {
                // if user is found then dont add the user again by email

                // converting to hash
                // adding the password
                findingUserByEmail.VerificationPassword = randomVerficationPasswordGenerateString.ToString();
                var userFoundVerificationPasswordChange = new User();
                userFoundVerificationPasswordChange = findingUserByEmail;
                _dataContext.Users.Update(userFoundVerificationPasswordChange);
                await _dataContext.SaveChangesAsync();
            }
            else
            { // if email is not found then add the email there and generate the random verifcation password for it and contact number
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


                var addingUser = new User();
                addingUser.Email = viewModel.Email;
                addingUser.VerificationPassword = randomVerficationPasswordGenerateString.ToString();
                addingUser.Created_At = DateTime.Now;
                addingUser.ContactNumber = randomContactPasswordGenerateString.ToString();

                try
                {
                   
                    await _dataContext.AddAsync(addingUser);
                    await _dataContext.SaveChangesAsync();

                }
                catch (DbUpdateException ex) // if unique value is founded in contact number then update the contact value there.
                {
                    addingUser.ContactNumber = addingUser.ContactNumber + addingUser.ContactNumber[addingUser.ContactNumber.Length - 1];
                    await _dataContext.AddAsync(addingUser);
                    await _dataContext.SaveChangesAsync();
                }

                generatedUserId = addingUser.UserID;
            }

            // sending email

            MailMessage msgObj = new MailMessage("officalpaenchat@gmail.com", viewModel.Email);
            msgObj.Subject = "Paen chat verification code";
            msgObj.IsBodyHtml = true;
            msgObj.Body = $@"<h1>Paen chat verification code:</h1>
            <p>Your Paen chat verification code is <strong>{randomVerficationPasswordGenerateString.ToString()}.</strong></p> ";


            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.EnableSsl = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential() { UserName = "officalpaenchat@gmail.com", Password = "ogvlsqxesbwmhbrm" };
            client.Send(msgObj);

            return Ok();
        }

        [HttpPost("{RandomCodeVerification}")]
        public async Task<IActionResult> RandomCodeVerification(CodeVerificationViewModel viewModel)
        {

            var validatingCurrentUserGeneratedCode = await _dataContext.Users.FirstOrDefaultAsync(a => a.Email == viewModel.Email);
            if (validatingCurrentUserGeneratedCode == null) return BadRequest("Sorry can't find your email");

            if (validatingCurrentUserGeneratedCode != null)
            {
                // checking the generate coded which is valid or not
                if(validatingCurrentUserGeneratedCode.VerificationPassword == viewModel.EnteredVerificationPassword)
                {
                    // user is validate means correct user
                    // assigin to that user that code is used
                    validatingCurrentUserGeneratedCode.VerificationPassword = "Code Used";
                    validatingCurrentUserGeneratedCode.EmailVerification = true;
                    _dataContext.Update(validatingCurrentUserGeneratedCode);
                    await _dataContext.SaveChangesAsync();

                    // generate a token
                    // creatig jwt token
                    List<Claim> myClaims = new List<Claim>
                    {
                        new Claim("UserId", validatingCurrentUserGeneratedCode.UserID.ToString()),
                    };
                    var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                        configuration.GetSection("ApplicationSettings:JWT_Secret").Value));

                    var signingCredentials = new SigningCredentials (key, SecurityAlgorithms.HmacSha256);

                    var token = new JwtSecurityToken(
                        claims: myClaims,
                        expires: DateTime.Now.AddDays(7),
                        signingCredentials: signingCredentials
                        );

                    var jwt = new JwtSecurityTokenHandler().WriteToken(token);
                    return Ok(jwt);



                }
                else if(validatingCurrentUserGeneratedCode.VerificationPassword != viewModel.EnteredVerificationPassword)
                {
                    return BadRequest("Sorry code is incorrect, please try again");
                }
            }

            return Ok();
        }

        [HttpPost("AuthorizedPerson")]
        public async Task<IActionResult> AuthorizedPerson(GetTokenViewModel getTokenViewMode)
        {
            if (getTokenViewMode == null)
                return BadRequest("token is not found");
            // decode the sended token
            var user = new User();
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(getTokenViewMode.Token);
            var tokenS = jsonToken as JwtSecurityToken;
            var gettingDataFromTokenObj = tokenS?.Payload;
            foreach (var singleProp in gettingDataFromTokenObj)
            {
                if (singleProp.Key == "UserId")
                {
                    user = await _dataContext.Users
                        .FirstOrDefaultAsync(a=>a.UserID == Convert.ToInt32(singleProp.Value));
                    break;
                }
            }
            return Ok(new
            {
                userId = user.UserID,
                userName = user.UserName,
                userContactNumber = user.ContactNumber,
                userProfilePhoto = user.ProfilePhotoUrl,
                userStatus = user.About,
            });
        }

        [HttpGet("FetchingDataForFormUser/{userId}")]
        public async Task<IActionResult> FetchingDataForFormUser(int userId)
        {

            var fetchingDataFromServer = await _dataContext.Users.FirstOrDefaultAsync(a => a.UserID == userId);
            return Ok(new
            {
                UserName = fetchingDataFromServer.UserName,
                AboutStatus = fetchingDataFromServer.About,
                ProfilePhotoUrl = fetchingDataFromServer.ProfilePhotoUrl
            });
        }

        [HttpPut("AddingUserProfileData")]
        public async Task<IActionResult> AddingUserProfileData([FromForm] AddUserInfoViewModel addUserInfo)
        {
            if(addUserInfo.File == null)
            {
                var updatingUserProfileData = await _dataContext.Users.FirstOrDefaultAsync(a => a.UserID == addUserInfo.UserId);
                updatingUserProfileData.UserName = addUserInfo.UserName;
                updatingUserProfileData.About = addUserInfo.AboutStatus;
                _dataContext.Update(updatingUserProfileData);
                await _dataContext.SaveChangesAsync();
            }else
            {
                var updatingUserProfileData = await _dataContext.Users.FirstOrDefaultAsync(a => a.UserID == addUserInfo.UserId);
                updatingUserProfileData.UserName = addUserInfo.UserName;
                updatingUserProfileData.About = addUserInfo.AboutStatus;
                // store into cloudinary that image
                var uploadResult = new ImageUploadResult();

                using (var stream = addUserInfo.File.OpenReadStream())
                {
                    var uploadparams = new ImageUploadParams
                    {
                        File = new FileDescription(addUserInfo.File.Name, stream)
                    };
                    uploadResult = _cloudinary.Upload(uploadparams);
                }
               updatingUserProfileData.PublicId = uploadResult.PublicId;
               updatingUserProfileData.ProfilePhotoUrl = uploadResult.Url.ToString();

                _dataContext.Update(updatingUserProfileData);
                await _dataContext.SaveChangesAsync();
            }
            return Ok();

        }

       





    }
}
