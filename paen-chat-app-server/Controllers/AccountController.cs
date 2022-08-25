using DataAccess.DataContext_Class;
using DataAccess.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Presentation.ViewModel;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;

namespace paen_chat_app_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private DataContext _dataContext;
        private readonly IConfiguration configuration;

        public AccountController(DataContext dataContext, IConfiguration configuration)
        {
            _dataContext = dataContext;
            this.configuration = configuration;
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

            return Ok(new
            {
                UserId = findingUserByEmail != null ? findingUserByEmail.UserID : generatedUserId
            });
        }

        [HttpPost("{RandomCodeVerification}")]
        public async Task<IActionResult> RandomCodeVerification(CodeVerificationViewModel viewModel)
        {
            var validatingCurrentUserGeneratedCode = await _dataContext.Users.FirstOrDefaultAsync(a => a.UserID == viewModel.UserId);
            if (validatingCurrentUserGeneratedCode == null) return BadRequest("Sorry can't find your email");

            if (validatingCurrentUserGeneratedCode != null)
            {
                // checking the generate coded which is valid or not
                if(validatingCurrentUserGeneratedCode.VerificationPassword == viewModel.EnteredVerificationPassword)
                {
                    // user is validate means correct user
                    // assigin to that user that code is used
                    validatingCurrentUserGeneratedCode.VerificationPassword = "Code Used";
                    _dataContext.Update(validatingCurrentUserGeneratedCode);
                    await _dataContext.SaveChangesAsync();

                    // generate a token
                    // creatig jwt token
                    List<Claim> myClaims = new List<Claim>
                    {
                        new Claim("UserId", viewModel.UserId.ToString()),
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

       


    }
}
