using Business_Core.Entities;
using Business_Core.Some_Data_Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Core.IServices
{
    public interface IContactService
    {
        Task<string> ContactFoundAsync(int userId, string phoneNumber);
        Task<object> AddingContactAsync(Contact contact);
        Task<List<FetchingSingleUserContacts>> GetSingleUserAllContactsAsync(int userId);
        Task<object> ListOfAllChatConnectedWithSingleUserAsync(int userId);
        Task EditSingleContactAsync(Contact contact);
        Task BlockingSingleContactAsync(int contactId);
        Task UnlocakingSingleContactAsync(int contactId);

        Task AddConversationContactToConversationListAsync(string groupId);
    }
}
