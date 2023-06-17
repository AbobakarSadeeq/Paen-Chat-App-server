using Business_Core.Entities;
using Business_Core.IServices;
using Business_Core.IUnitOfWork;
using Business_Core.Some_Data_Classes;
using StackExchange.Redis;
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
        private readonly IUserRedisCacheService _redisUserCacheService;


        public ContactService(IUnitOfWork unitOfWork, IConnectionMultiplexer redis,
            IUserRedisCacheService redisUserCacheService)
        {
            _unitOfWork = unitOfWork;
            _redisUserCacheService = redisUserCacheService;

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

        public async Task<List<FetchingSingleUserContacts>> GetSingleUserAllContactsAsync(int userId)
        {
           var contactList = await _unitOfWork._contactRepository.GetSingleUserContactsAsync(userId);

            // storing data about i become online and set data inside the redis
           await _redisUserCacheService.UserAvailabilityStatusAddToSetRedis(userId.ToString());
            List<string> verifiedContactsGroupId = new List<string>();
            
            // below is checking which user contacct is online
            // now i have to store the connected and valid all using group Inside the hash ds in redis
            foreach (var singleContact in contactList)
            {
                if(singleContact.VerifiedContactUser == true &&
                  await _redisUserCacheService.UserAvailabilityStatusChecking(singleContact.UserId.ToString()) == true)
                {
                    // if Id become found in redis then consider it online
                    singleContact.UserAvailabilityStatus = true;

                }else
                {
                    // if Id become not found at redis then consider it offline
                    singleContact.UserAvailabilityStatus = false;
                }


                // now storing the data inside the hash ds

                if(singleContact.VerifiedContactUser == true)
                {
                    verifiedContactsGroupId.Add(singleContact.groupId);
                }

            }

            await _redisUserCacheService.StoringUserConnectedContactsGroupIdToRedisHashAsync(verifiedContactsGroupId, userId.ToString());

            return contactList;
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
