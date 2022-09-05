using DataAccess.DataContext_Class;
using DataAccess.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Presentation.ViewModel.Contact;

namespace paen_chat_app_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly DataContext _dataContext;
        public ContactController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpPost]
        public async Task<IActionResult> AddContact(AddContactViewModel viewModel)
        {
            // if user is already in list then show error.

            var isContactAlreadyExist = _dataContext.Contacts.FirstOrDefault(a => a.PhoneNumber == viewModel.ContactNo &&
            a.UserId == viewModel.UserId);

            if (isContactAlreadyExist != null)
            {
                return BadRequest("Sorry account is already connected with you so, cannot add it up again");
            }


            // first check the given contact is a user or not means using is he/she using it or not.
            var isContactIsValidUser = await _dataContext.Users.FirstOrDefaultAsync(a => a.ContactNumber == viewModel.ContactNo);
            var addContact = new List<Contact>()
            {
                new Contact
                {
                Created_At = DateTime.Now,
                Block_Contact = false,
                FirstName = viewModel.FirstName,
                PhoneNumber = viewModel.ContactNo,
                UserId = viewModel.UserId,
                LastName = viewModel.LastName
                }

            };

            if (isContactIsValidUser != null)
            {
                // it means it is a valid user
                addContact[0].Verified_Contact = true;

                // finding that user Id whos want to connect with the contect
                var findingUser = await _dataContext.Users.FirstOrDefaultAsync(a => a.UserID == viewModel.UserId);

                // adding that user you found its numeber and he/she didnt have your number connection.

                addContact.Add(new Contact
                {
                    Verified_Contact = true,
                    Created_At = DateTime.Now,
                    PhoneNumber = findingUser.ContactNumber,
                    Block_Contact = false,
                    UserId = isContactIsValidUser.UserID,
                });

            }
            else
            {
                addContact[0].Verified_Contact = false;
            }

            await _dataContext.Contacts.AddRangeAsync(addContact);
            await _dataContext.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetSingleUserContacts(int userId)
        {

            // performing the right joining and it is same as sql query
            // if any right table does not found that data where we are joining on it then we have to give the condition on it otherwise error like if contact on left table is not found that is on right then show left table show null and show right table its data 
            var RightJoining = await (from c in _dataContext.Contacts // right-table
                                      join u in _dataContext.Users // left - table
                                      on c.PhoneNumber equals u.ContactNumber into bothTableData
                                      from rightTable in bothTableData.DefaultIfEmpty()
                                      where c.UserId == userId
                                      select new
                                      {
                                          ContactId = c.ContactID,
                                          ContactName = c.FirstName + " " + c.LastName,
                                          PhoneNumber = c.PhoneNumber,
                                          VerifiedContactUser = c.Verified_Contact,
                                          BlockContact = c.Block_Contact,
                                          AboutStatus = rightTable.About == null ? null : rightTable.About,
                                          UserImage = rightTable.ProfilePhotoUrl == null ? null : rightTable.ProfilePhotoUrl,
                                          UserId = rightTable.UserID == null ? 0 : rightTable.UserID, // if right table doesnt have it then make it 0
                                      }).ToListAsync();



            return Ok(RightJoining);
        }


        [HttpGet("ListOfChatConnectedWithSingleUser/{userId}")]
        public async Task<IActionResult> ListOfChatConnectedWithSingleUser(int userId)
        {
            // connect the contact with users table and then filter by given userId 
            // when filtered then filterd again by last_message (means thats chats are doing the message)
            var RightJoining = await (from c in _dataContext.Contacts // right-table
                                      join u in _dataContext.Users // left - table
                                      on c.PhoneNumber equals u.ContactNumber into bothTableData
                                      from rightTable in bothTableData.DefaultIfEmpty()
                                      where c.UserId == userId && c.LastMessage != null
                                      select new
                                      {
                                          ContactId = c.ContactID,
                                          ContactName = c.FirstName.Length == 0 ? "" : c.FirstName + " " + c.LastName,
                                          PhoneNumber = c.PhoneNumber,
                                          BlockContact = c.Block_Contact,
                                          UserImage = rightTable.ProfilePhotoUrl == null ? null : rightTable.ProfilePhotoUrl,
                                          LastMessage = c.LastMessage,
                                      }).ToListAsync();




            return Ok(RightJoining);
        }

    }
}
