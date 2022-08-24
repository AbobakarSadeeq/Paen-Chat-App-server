using DataAccess.DataContext_Class;
using DataAccess.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Presentation.ViewModel;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace paen_chat_app_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private DataContext _dataContext;
        public AccountController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpPost]

        public async Task<IActionResult> RegisteringUser(UserLogInViewModel viewModel)
        {
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

                


            }



            // sending email

            MailMessage msgObj = new MailMessage("officalpaenchat@gmail.com", viewModel.Email);
            msgObj.Subject = "Paen Chat verification code";
            msgObj.IsBodyHtml = true;
            msgObj.Body = $@"<h1>Paen chat verification code:</h1>
            <p>Your Paen chat verification code is <strong>{randomVerficationPasswordGenerateString.ToString()}.</strong></p> ";


            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.EnableSsl = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential() { UserName = "officalpaenchat@gmail.com", Password = "ogvlsqxesbwmhbrm" };
            client.Send(msgObj);

            return Ok("User founded and random verifcation number generated and sended via email");

        }


    }
}
