using Business_Core.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Core.IUnitOfWork
{
    public interface IUnitOfWork
    {
        public IContactRepository _contactRepository { get; }
        public IUserRepository _userRepository { get; }
        public IMessageRepository _messageRepository { get; }

        Task<int> CommitAsync();
        void Dispose();
    }
}
