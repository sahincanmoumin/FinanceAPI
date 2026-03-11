using BusinessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.Pagination;
using EntityLayer.DTOs.User;
using EntityLayer.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiLayer.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : BaseController
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = GetUserId();
            var result = await _userService.GetByIdAsync(userId);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> GetAll([FromQuery] UserFilterDto filter)
        {
            var result = await _userService.GetAllUserAsync(filter);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (!IsAdmin() && id != GetUserId())
                throw new BusinessException(ErrorKeys.Unauthorized);

            var result = await _userService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UserUpdateDto dto)
        {
            if (!IsAdmin() && dto.Id != GetUserId())
                throw new BusinessException(ErrorKeys.Unauthorized);

            await _userService.UpdateAsync(dto);
            _logger.LogInformation("User updated. UserId: {UserId}", dto.Id);
            return Ok(new { Message = "User updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin() && id != GetUserId())
                throw new BusinessException(ErrorKeys.Unauthorized);

            await _userService.DeleteUserAsync(id);
            _logger.LogInformation("User deleted. UserId: {UserId}", id);
            return Ok(new { Message = "User deleted successfully." });
        }
    }
}