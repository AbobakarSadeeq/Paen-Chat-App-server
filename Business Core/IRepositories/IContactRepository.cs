using Business_Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Core.IRepositories
{
    public interface IContactRepository
    {
        Task<string> ContactExistFound(Contact contact);
        Task AddContactAsync(Contact contact);
        Task<object> GetSingleUserContactsAsync(int userId);
        Task<object> ListOfChatConnectedWithSingleUserAsync(int userId);
        Task EditContactAsync(Contact contact);
        Task BlockingContactAsync(int contactId);
        Task UnlocakingContactAsync(int contactId);

        Task AddContactConversationToConversationList(string conversationGroupId);
    }
}
