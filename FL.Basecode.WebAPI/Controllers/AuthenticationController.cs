using FL.Basecode.DTOs;
using FL.Basecode.DTOs.Authentication;
using FL.Basecode.DTOs.Authentication.Register;
using FL.Basecode.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FL.Basecode.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("register/google/login")]
        public async Task<IActionResult> RegisterWithGoogle([FromBody] GoogleAuthRequest request)
        {
            var result = await _authService.RegisterWithGoogleAsync(request);
            return StatusCode(result.StatusCode, result);
        }
    }

}
