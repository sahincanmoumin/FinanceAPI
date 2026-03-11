using BusinessLayer.Abstract;
using EntityLayer.DTOs.Pagination;
using EntityLayer.DTOs.Role;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiLayer.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : BaseController
    {
        private readonly IRoleService _roleService;
        private readonly IUserRoleService _userRoleService;
        private readonly ILogger<RolesController> _logger;

        public RolesController(IRoleService roleService, IUserRoleService userRoleService, ILogger<RolesController> logger)
        {
            _roleService = roleService;
            _userRoleService = userRoleService; 
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] RoleFilterDto filter)
        {
            var result = await _roleService.GetAllRolesAsync(filter);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
        {
            await _roleService.AddAsync(dto);
            _logger.LogInformation("New role created by Admin: {RoleName}", dto.Name);
            return Ok(new { Message = "Role created successfully." });
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserRoles(int userId, [FromQuery] UserRoleFilterDto filter)
        {
            var result = await _userRoleService.GetRolesByUserIdAsync(userId, filter);
            return Ok(result);
        }
    }
}