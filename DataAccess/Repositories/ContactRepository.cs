﻿using Business_Core.Entities;
using Business_Core.IRepositories;
using Business_Core.Some_Data_Classes;
using DataAccess.DataContext_Class;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class ContactRepository : IContactRepository
    {
        private readonly DataContext _DataContext;
        public ContactRepository(DataContext DataContext)
        {
            _DataContext = DataContext;
        }

        #region AddingContect
        public async Task<string> ContactExistFound(Contact contact)
        {
            var isContactAlreadyExist = await _DataContext.Contacts.FirstOrDefaultAsync(a => a.PhoneNumber == contact.PhoneNumber &&
                      a.UserId == contact.UserId);

            if (isContactAlreadyExist != null)
            {
                return "Sorry account is already connected with you so, cannot add it up again";
            }

            return "";
        }

        public async Task<object> AddContactAsync(Contact contact)
        {

            // first check the given contact is a user or not means using is he/she using it or not.
            // if you are saying that if user added a contact and not verifyied but after sometime it will be verfiyed and here its not possible to do that because the contact number and user must be first verified.
            var isContactIsValidUser = await _DataContext.Users.FirstOrDefaultAsync(a => a.ContactNumber == contact.PhoneNumber);
            var findingUser = await _DataContext.Users.FirstOrDefaultAsync(a => a.UserID == contact.UserId);

            var addContact = new List<Contact>()
            {
              contact
            };

            if (isContactIsValidUser != null)
            {
                // it means it is a valid user
                addContact[0].Verified_Contact = true;
                addContact[0].ConnectedInMessages = true;

                // finding that user Id whos want to connect with the contect

                // adding that user you found its numeber and he/she didnt have your number connection.

                addContact.Add(new Contact
                {
                    Verified_Contact = true,
                    Created_At = DateTime.Now,
                    PhoneNumber = findingUser.ContactNumber,
                    Block_Contact = false,
                    UserId = isContactIsValidUser.UserID,
                    UserGroupPrivateConnectionId = addContact[0].UserGroupPrivateConnectionId,
                    ConnectedInMessages = true // which means when a user become connected and it is valid user then add them into connectedInMessages
                });

            }
            else
            {
                addContact[0].Verified_Contact = false;
            }

            try
            {

                await _DataContext.Contacts.AddRangeAsync(addContact);
                await _DataContext.SaveChangesAsync();

            }
            catch (DbUpdateException ex) // if unique value is founded in contact number then update the contact value there.
            {
                addContact[0].UserGroupPrivateConnectionId = addContact[0].UserGroupPrivateConnectionId + addContact[0].UserGroupPrivateConnectionId[addContact[0].UserGroupPrivateConnectionId.Length - 1];
                await _DataContext.Contacts.AddRangeAsync(addContact);
                await _DataContext.SaveChangesAsync();
            }

            
            var userWantsToConnectItselfDetail = new
            {
                ContactName = " ",
               // AboutStatus = findingUser.About,
                BlockContact = false,
                ConnectedInMessages = true,
                ContactId = addContact[1].ContactID,
                PhoneNumber = findingUser.ContactNumber,
                UserId = findingUser.UserID,
                VerifiedContactUser = true,
                UserImage = findingUser.ProfilePhotoUrl,
                groupId = addContact[0].UserGroupPrivateConnectionId
            };

            var userAddedContactDetail =  new
            {
                ContactName = addContact[0].FirstName + " " + addContact[0].LastName,
              //  AboutStatus = isContactIsValidUser == null ? "": isContactIsValidUser.About,
                BlockContact = addContact[0].Block_Contact,
                ConnectedInMessages = addContact[0].ConnectedInMessages,
                ContactId = addContact[0].ContactID,
                PhoneNumber = addContact[0].PhoneNumber,
                UserId = isContactIsValidUser == null ? 0 : isContactIsValidUser.UserID,
                VerifiedContactUser = addContact[0].Verified_Contact,
                UserImage = isContactIsValidUser == null ? "": isContactIsValidUser.ProfilePhotoUrl,
                groupId = addContact[0].UserGroupPrivateConnectionId,
                BlockContactByConnectedUser = false,
                UserAvailabilityStatus = false,
                LastMessageOfSingleContact = "",
            };

            return new
            {
                UserWantsToConnectItselfDetail = userWantsToConnectItselfDetail != null ? userWantsToConnectItselfDetail : null,
                UserAddedContactDetail = userAddedContactDetail,
            };
        }

        #endregion

        public async Task BlockingContactAsync(int contactId)
        {
            var findingContact = await _DataContext.Contacts.FirstOrDefaultAsync(a => a.ContactID == contactId);
            findingContact.Block_Contact = true; // for block and unlock
            _DataContext.Contacts.Update(findingContact);
        }

        public async Task EditContactAsync(Contact contact)
        {
            var findingContactId = await _DataContext.Contacts
                .FirstOrDefaultAsync(a => a.ContactID == contact.ContactID);
            findingContactId.FirstName = contact.FirstName;
            findingContactId.LastName = contact.LastName;
            _DataContext.Contacts.Update(findingContactId);

           

        }

        public async Task<List<FetchingSingleUserContacts>> GetSingleUserContactsAsync(int userId)
        {
            // performing the right joining and it is same as sql query
            // if any right table does not found that data where we are joining on it then we have to give the condition on it otherwise error like if contact on left table is not found that is on right then show left table show null and show right table its data 
            //var RightJoining = await (from c in _DataContext.Contacts // right-table
            //                         join u in _DataContext.Users // left - table
            //                         on c.PhoneNumber equals u.ContactNumber into bothTableData
            //                         from rightTable in bothTableData.DefaultIfEmpty()
            //                         where c.UserId == userId
            //                         select new FetchingSingleUserContacts
            //                         {
            //                             ContactId = c.ContactID,
            //                             ContactName = c.FirstName + " " + c.LastName,
            //                             PhoneNumber = c.PhoneNumber,
            //                             VerifiedContactUser = c.Verified_Contact,
            //                             AboutStatus = rightTable.About == null ? null : rightTable.About,
            //                             UserImage = rightTable.ProfilePhotoUrl == null ? null : rightTable.ProfilePhotoUrl,
            //                             UserId = rightTable.UserID == null ? 0 : rightTable.UserID,


            //                             BlockContact = c.Block_Contact, // this is userItself blocking value like when that same user logged in this will show user-blocks.


            //                             BlockContactByConnectedUser = rightTable.Contacts.FirstOrDefault(a => a.UserId == rightTable.UserID && a.UserGroupPrivateConnectionId == c.UserGroupPrivateConnectionId).Block_Contact ? 
            //                             rightTable.Contacts.FirstOrDefault(a => a.UserId == rightTable.UserID && a.UserGroupPrivateConnectionId == c.UserGroupPrivateConnectionId).Block_Contact : false, // connected user-block value



            //                             ConnectedInMessages = c.ConnectedInMessages,
            //                             groupId = c.UserGroupPrivateConnectionId
            //                         }).ToListAsync();




            //return RightJoining;
            return new List<FetchingSingleUserContacts>();
        }


        // for not this is not required because i dont want to execute two time fetching contacts from database
        // this will execute when chat app is opend and if user is in the converstion list then it will fetch all those users then fetch all messages of each user required to change fetching messages.
        
        public async Task<object> ListOfChatConnectedWithSingleUserAsync(int userId)
        {
            //    var RightJoining = await (from c in _DataContext.Contacts // right-table
            //                             join u in _DataContext.Users // left - table
            //                             on c.PhoneNumber equals u.ContactNumber into bothTableData

            //                             from rightTable in bothTableData.DefaultIfEmpty()
            //                             where c.UserId == userId  && c.Block_Contact == false
            //                             select new
            //                             {
            //                                 ContactId = c.ContactID,
            //                                 SingleContactGroupConnectionId = c.UserGroupPrivateConnectionId,
            //                                 ContactName = c.FirstName.Length == 0 ? "" : c.FirstName + " " + c.LastName,
            //                                 PhoneNumber = c.PhoneNumber,
            //                                 BlockContact = c.Block_Contact,
            //                                 UserImage = rightTable.ProfilePhotoUrl == null ? null : rightTable.ProfilePhotoUrl,
            //                                 UserItSelfId = userId,
            //                                 UsersConnectedId = rightTable.UserID,
            //                                 SingleConnectedUserMessagesList = new List<Message>()
            //                             }).ToListAsync();

            //    // changing required here for to not fetch all messages at a time
            //    foreach (var singleContact in RightJoining)
            //    {
            //        var findingSingleContactAllMessages = _DataContext.Messages
            //            .Include(a => a.MessageAttachments)
            //            .Where(a => (a.SenderId == userId && a.ReceiverId == singleContact.UsersConnectedId) ||
            //            (a.SenderId == singleContact.UsersConnectedId && a.ReceiverId == userId))
            //            .ToList();
            //        if (findingSingleContactAllMessages != null)
            //        {
            //            singleContact.SingleConnectedUserMessagesList.AddRange(findingSingleContactAllMessages);
            //        }
            //    }

            //    return RightJoining;
            return new
            {

            };
        }

        public async Task UnlocakingContactAsync(int contactId)
        {
            var findingContact = await _DataContext.Contacts.FirstOrDefaultAsync(a => a.ContactID == contactId);
            findingContact.Block_Contact = false; // for block and unlock
            _DataContext.Contacts.Update(findingContact);

        }

        public async Task ConnectBothUserInConnectedMessageSection(string conversationGroupId)
        {
            var findingSingleConversationUsers = await _DataContext.Contacts.Where(a=>a.UserGroupPrivateConnectionId == conversationGroupId)
                .Take(2).ToListAsync();
            if(findingSingleConversationUsers[0].ConnectedInMessages == false ||
                findingSingleConversationUsers[1].ConnectedInMessages == false)
            {
                findingSingleConversationUsers[0].ConnectedInMessages = true;
                findingSingleConversationUsers[1].ConnectedInMessages = true;
            }
            _DataContext.Contacts.UpdateRange(findingSingleConversationUsers);
        }

  

            
            
        
    }
}
