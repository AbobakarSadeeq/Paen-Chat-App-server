using AutoMapper;
using Business_Core.Entities;
using Presentation.ViewModel;
using Presentation.ViewModel.Contact;
using Presentation.ViewModel.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.AutoMapper
{
    public class AutoMap : Profile
    {
        public AutoMap()
        {
            CreateMap<EditContactViewModel, Contact>().ReverseMap();
            CreateMap<UserLogInViewModel, User>().ReverseMap();
            CreateMap<AddUserInfoViewModel, AddUserInfo>().ReverseMap();
            CreateMap<ClientSingleMessageViewModel, ClientMessageRedis>().ReverseMap();
        }
    }
}
