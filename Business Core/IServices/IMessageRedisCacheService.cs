using Business_Core.Entities;
using Business_Core.FunctionParametersClasses;
using Business_Core.Some_Data_Classes;

namespace Business_Core.IServices
{
    public interface IMessageRedisCacheService
    {
        Task<List<Message>> SaveMessagesInRedisAsync(ClientMessageRedis message, string groupId);
        Task<FetchingMessagesForUser> FetchingSingleConversationUsersMessagesFromRedis(SingleConversationMessagesParams funcParams);
        Task<FetchingMessagesForUser> FetchingSingleConversationUsersMessagesFromDb(SingleConversationMessagesParams funcParams, List<Message> dbMessages);
    }
}
