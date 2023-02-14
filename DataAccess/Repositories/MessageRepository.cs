using Business_Core.Entities;
using Business_Core.IRepositories;
using DataAccess.DataContext_Class;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        // we will write all here about the database and crud of message to db.
        private readonly DataContext _dataContext;

        public MessageRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
            
        }

        public async Task<List<Message>> FetchingSingleConversationAllMessagesAsync(int user1, int user2)
        {
            return await _dataContext.Messages.Where(a => a.SenderId == user1 && a.ReceiverId == user2 || 
            a.SenderId == user2 && a.ReceiverId == user1).ToListAsync();
        }

        public async Task StoringUsersMessages(List<Message> messages)
        {
            await _dataContext.Messages.AddRangeAsync(messages);
        }
    }
}
