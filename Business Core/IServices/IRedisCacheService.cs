using Business_Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Core.IServices
{
    public interface IRedisCacheService
    {
        Task SaveMessageToHash(ClientMessageRedis message, string groupId);
        Task SaveMessagesToDb();

    }
}
