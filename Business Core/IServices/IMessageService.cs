using Business_Core.Entities;
using Business_Core.FunctionParametersClasses;
using Business_Core.Some_Data_Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Core.IServices
{
    public interface IMessageService
    {
        Task StoringUsersMessagesAsync(List<Message> usersAllMessagesList);
        Task<FetchingMessagesForUser> GetSingleConversationAllMessagesFromRedisAndDbAsync(SingleConversationMessagesParams fetchingSpecificMessageParams);
    }
}
