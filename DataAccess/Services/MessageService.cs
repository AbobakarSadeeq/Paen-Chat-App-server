using Business_Core.Entities;
using Business_Core.IServices;
using Business_Core.IUnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Services
{
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MessageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Message>> GetSingleConversationMessagesAllListAsync(int user1, int user2)
        {
            return await _unitOfWork._messageRepository.FetchingSingleConversationAllMessagesAsync(user1, user2);
            
        }

        public async Task StoringUsersMessagesAsync(List<Message> usersAllMessagesList)
        {
            await _unitOfWork._messageRepository.StoringUsersMessages(usersAllMessagesList);
            await _unitOfWork.CommitAsync();
        }
    }
}
