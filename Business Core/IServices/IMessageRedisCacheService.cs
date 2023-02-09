﻿using Business_Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Core.IServices
{
    public interface IMessageRedisCacheService
    {
        Task<List<Message>> SaveMessageToHashAsync(ClientMessageRedis message, string groupId);
        Task GetSingleUserAllConnectedWithUsers();
       
    }
}
