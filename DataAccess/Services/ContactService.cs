﻿using Business_Core.Entities;
using Business_Core.IServices;
using Business_Core.IUnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Services
{
    public class ContactService : IContactService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ContactService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region addContactService
        public async Task<string> ContactFoundAsync(int userId, string phoneNumber)
        {
            Contact contact = new Contact() { UserId = userId, PhoneNumber = phoneNumber };
            return await _unitOfWork._contactRepository.ContactExistFound(contact);
        }

        public async Task<object> AddingContactAsync(Contact contact)
        {
           var addedContactData =  await _unitOfWork._contactRepository.AddContactAsync(contact);
           // await _unitOfWork.CommitAsync();
            return addedContactData;
        }
        #endregion

        public async Task BlockingSingleContactAsync(int contactId)
        {

            await _unitOfWork._contactRepository.BlockingContactAsync(contactId);
            await _unitOfWork.CommitAsync();

        }

        public async Task EditSingleContactAsync(Contact contact)
        {
            await _unitOfWork._contactRepository.EditContactAsync(contact);
            await _unitOfWork.CommitAsync();
        }

        public async Task<object> GetSingleUserAllContactsAsync(int userId)
        {
           return await _unitOfWork._contactRepository.GetSingleUserContactsAsync(userId);
        }

        public async Task<object> ListOfAllChatConnectedWithSingleUserAsync(int userId)
        {
            throw new Exception();
          //  return await _unitOfWork._contactRepository.ListOfChatConnectedWithSingleUserAsync(userId);

        }

        public async Task UnlocakingSingleContactAsync(int contactId)
        {
            await _unitOfWork._contactRepository.UnlocakingContactAsync(contactId);
            await _unitOfWork.CommitAsync();
        }

        public async Task AddConversationContactToConversationListAsync(string groupId)
        {
            await _unitOfWork._contactRepository.ConnectBothUserInConnectedMessageSection(groupId);
            await _unitOfWork.CommitAsync();
        }
    }
}
