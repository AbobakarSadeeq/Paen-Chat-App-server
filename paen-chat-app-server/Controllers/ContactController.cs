using AutoMapper;
using Business_Core.Entities;
using Business_Core.IServices;
using Microsoft.AspNetCore.Mvc;
using Presentation.ViewModel.Contact;

namespace paen_chat_app_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IContactService _contactService;
        private readonly IMapper _mapper;
        public ContactController(IContactService contactService, IMapper mapper)
        {
            _contactService = contactService;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> AddContact(AddContactViewModel viewModel)
        {
            // if user is already in list then show error.

            var findingIsContactExist = await _contactService.ContactFoundAsync(viewModel.UserId, viewModel.ContactNo);
            if (findingIsContactExist != String.Empty)
            {
                return BadRequest(findingIsContactExist);
            }

            // making connection randomize characters for users to connect with their private chats.
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string randomizeChars = new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            var addContact = new Contact()
            {
                Created_At = DateTime.Now,
                Block_Contact = false,
                FirstName = viewModel.FirstName,
                PhoneNumber = viewModel.ContactNo,
                UserId = viewModel.UserId,
                LastName = viewModel.LastName,
                UserGroupPrivateConnectionId = randomizeChars

            };

            var newContact = await _contactService.AddingContactAsync(addContact);

            return Ok(newContact);
        }

        [HttpGet("{userId}")] // correcto
        public async Task<IActionResult> GetSingleUserContacts(int userId)
        {
            var findingSingleUserContacts = await _contactService.GetSingleUserAllContactsAsync(userId);
            return Ok(findingSingleUserContacts);
        }

        [HttpPut("EditContact")] // correcto
        public async Task<IActionResult> EditContact(EditContactViewModel viewModel)
        {
            var convertingToEntity = _mapper.Map<Contact>(viewModel);
            await _contactService.EditSingleContactAsync(convertingToEntity);
            return Ok();
        }

        [HttpGet("BlockingContact/{contactId}")]  // correcto
        public async Task<IActionResult> BlockingContact(int contactId)
        {
            await _contactService.BlockingSingleContactAsync(contactId);
            return Ok();
        }

        [HttpGet("UnlockingContact/{contactId}")] // correcto
        public async Task<IActionResult> UnlockingContact(int contactId)
        {
            await _contactService.UnlocakingSingleContactAsync(contactId);
            return Ok();
        }

        [HttpGet("MakeValidConnectedInMessageBetweenUser/{groupId}")]
        public async Task<IActionResult> MakeTrueConnectedInMessageBetweenUser(string groupId)
        {
            await _contactService.AddConversationContactToConversationListAsync(groupId);
            return Ok();
        }

    }
}
