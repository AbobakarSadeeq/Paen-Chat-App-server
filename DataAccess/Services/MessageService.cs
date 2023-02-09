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
        public async Task StoringUsersMessagesAsync(List<Message> usersAllMessagesList)
        {
            await _unitOfWork._messageRepository.StoringUsersMessages(usersAllMessagesList);
            await _unitOfWork.CommitAsync();
        }
    }
}
