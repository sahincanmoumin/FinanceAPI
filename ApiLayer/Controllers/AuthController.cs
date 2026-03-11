using BusinessLayer.Abstract;
using EntityLayer.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;

namespace ApiLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            _logger.LogInformation("New user registration attempt: {UserName}", dto.UserName);
            await _authService.RegisterAsync(dto);
            return Ok(new { Message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            _logger.LogInformation("Login attempt for user: {UserName}", dto.UserName);
            var token = await _authService.LoginAsync(dto);
            return Ok(new { Token = token, Message = "Login successful." });
        }
    }
}