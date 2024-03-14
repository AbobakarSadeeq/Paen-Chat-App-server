using Business_Core.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Presentation.ViewModel;

namespace paen_chat_app_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserRedisCacheService _userRedisCacheService;
        public AuthController(IUserRedisCacheService userRedisCacheService)
        {
            _userRedisCacheService = userRedisCacheService;
        }

        [HttpPost]
        [Route("UserVerificationSendCode")]
        public async Task<IActionResult> UserVerificationSendCode([FromBody] UserVerification userVerification)
        {
            await _userRedisCacheService.UserPhoneNumberVerificationSendCodeAsync(userVerification.PhoneNumber);
            return Ok();
        }

        [HttpPost]
        [Route("UserVerifyingSendedCode")]
        public async Task<IActionResult> UserVerifyingSendedCode([FromBody] UserVerification verification)
        {
            bool isUserVerified =  await _userRedisCacheService.UserVerifyingSendedCodeAsync(verification.VerificationCode, verification.PhoneNumber);
            return Ok(isUserVerified);

        }

    }
}
