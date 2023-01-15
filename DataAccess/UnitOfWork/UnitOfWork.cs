using Business_Core.IRepositories;
using Business_Core.IUnitOfWork;
using DataAccess.DataContext_Class;
using DataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext _DataContext;

        public IContactRepository _contactRepository { get; init; }
        public IUserRepository _userRepository { get; init; }
        public IMessageRepository _messageRepository { get; init; }

        public UnitOfWork(DataContext DataContext)
        {
            _DataContext = DataContext;

            _contactRepository = new ContactRepository(_DataContext);
            _userRepository = new UserRepository(_DataContext);
            _messageRepository = new MessageRepository(_DataContext);

        }

        public async Task<int> CommitAsync()
        {
            return await _DataContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            _DataContext.Dispose();
        }
    }
}
