using Business_Core.Entities;
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
        Task<object> GetSingleUserAllContactsAsync(int userId);
        Task<object> ListOfAllChatConnectedWithSingleUserAsync(int userId);
        Task EditSingleContactAsync(Contact contact);
        Task BlockingSingleContactAsync(int contactId);
        Task UnlocakingSingleContactAsync(int contactId);

        Task AddConversationContactToConversationListAsync(string groupId);
    }
}
