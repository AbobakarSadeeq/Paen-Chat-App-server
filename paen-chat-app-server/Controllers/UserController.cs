using Business_Core.Entities;
using Business_Core.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Presentation.ViewModel;

namespace paen_chat_app_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRedisCacheService _userRedisCacheService;
        private readonly IImageCloudStorageService _imageStorageService;
        public UserController(IUserRedisCacheService userRedisCacheService, IImageCloudStorageService imageStorageService)
        {
            _userRedisCacheService = userRedisCacheService;
            _imageStorageService = imageStorageService;
        }

        [HttpPost]
        [Route("AddUser")]
        public async Task<IActionResult> AddUser([FromForm] UserViewModel userView)
        {
            string profilePicUrl = await _imageStorageService.StoringImageToCloudAsync(userView.File); // storing image to cloud
            User redisAddUser = convertingUserViewModelToModel(userView, profilePicUrl); // mapping the entity
            await _userRedisCacheService.AddNewUserToRedisAsync(redisAddUser); // store single_user info to redis_db

            // check and convert json to actual object and see is it image url is valid.
            // if user already there but user want to update their profile like name and photo then what?
            
            return Ok();
        }

        private User convertingUserViewModelToModel(UserViewModel viewModel, string profilPicUrl)
        {
            var user = new User();
            user.AboutStatus = viewModel.AboutStatus;
            user.ContactNumber = viewModel.ContactNumber;
            user.FullName = viewModel.FullName;
            user.ProfilePhotoUrl = profilPicUrl;
            return user;

        }
    }
}
