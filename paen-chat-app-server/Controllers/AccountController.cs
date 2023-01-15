using CloudinaryDotNet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Presentation.AppSettings;
using Presentation.ViewModel;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Business_Core.IServices;
using Business_Core.Entities;

namespace paen_chat_app_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public AccountController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }
 

       

        [HttpPost]
        // this algo having add new user and assign random generate contact-number to database and generate a verfication code and store in database and send verfication code to gmail.
        public async Task<IActionResult> RegisteringUser(UserLogInViewModel viewModel)
        {
           var convertingToEntity = _mapper.Map<User>(viewModel);
            await _userService.RegistrationUser(convertingToEntity);
            return Ok();
        }

        [HttpPost("{RandomCodeVerification}")]
        public async Task<IActionResult> RandomCodeVerification(CodeVerificationViewModel viewModel)
        {
            var token = await _userService.VerifyingVerificationCodeAsync(viewModel.Email, viewModel.EnteredVerificationPassword);
            if(token == null)
            {
            return BadRequest("Sorry code is incorrect, please try again");
            }

            return Ok(token);
        }

        [HttpPost("AuthorizedPerson")]
        [Authorize]
        public async Task<IActionResult> AuthorizedPerson(GetTokenViewModel getTokenViewMode)
        {
            if (getTokenViewMode.Token == null)
                return BadRequest("token is not found");
         
            var userInfo = await _userService.AuthorizedUserInfoAsync(getTokenViewMode.Token);
            return Ok(userInfo);
            
        }

        // used for to get the user info when he/she clicked on their profile
        [HttpGet("FetchingDataForFormUser/{userId}")]
        [Authorize]
        public async Task<IActionResult> FetchingDataForFormUser(int userId)
        {
            var userProfileInfoFetched = await _userService.UserProfileInfoAsync(userId);
            return Ok(userProfileInfoFetched);
        }

        [HttpPut("AddingUserProfileData")]
        [Authorize]
        public async Task<IActionResult> AddingUserProfileData([FromForm] AddUserInfoViewModel addUserInfo)
        {
            var convertingToEntity = _mapper.Map<AddUserInfo>(addUserInfo);
            await _userService.AddUserProfileAsync(convertingToEntity);
            return Ok();

        }

       





    }
}
