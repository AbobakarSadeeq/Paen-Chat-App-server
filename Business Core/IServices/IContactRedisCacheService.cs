using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Core.IServices
{
    public interface IContactRedisCacheService
    {
        Task StoringSingleContactTo();
    }
}
