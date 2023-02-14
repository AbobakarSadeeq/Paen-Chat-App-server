using Business_Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Core.IRepositories
{
    public interface IMessageRepository
    {
        Task StoringUsersMessages(List<Message> messages);
        Task<List<Message>> FetchingSingleConversationAllMessagesAsync(int user1, int user2);
    }
}
