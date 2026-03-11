using BusinessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.CurrentAccount;
using EntityLayer.DTOs.Pagination;
using EntityLayer.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiLayer.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CurrentAccountsController : BaseController
    {
        private readonly ICurrentAccountService _currentAccountService;
        private readonly ICompanyService _companyService;
        private readonly ILogger<CurrentAccountsController> _logger;

        public CurrentAccountsController(ICurrentAccountService currentAccountService, ICompanyService companyService, ILogger<CurrentAccountsController> logger)
        {
            _currentAccountService = currentAccountService;
            _companyService = companyService;
            _logger = logger;
        }

        private async Task<bool> HasAccessToCompany(int companyId)
        {
            if (User.IsInRole("Admin")) return true;
            var company = await _companyService.GetByIdAsync(companyId);
            int loggedInUserId = GetUserId();
            return company != null && company.UserId == loggedInUserId;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] CurrentAccountFilterDto filter, [FromQuery] int companyId)
        {
            if (!await HasAccessToCompany(companyId))
                throw new BusinessException(ErrorKeys.Unauthorized);

            var result = await _currentAccountService.GetAllCurrentAccountsAsync(filter, companyId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var account = await _currentAccountService.GetByIdAsync(id);
            if (!await HasAccessToCompany(account.CompanyId))
                throw new BusinessException(ErrorKeys.Unauthorized);

            return Ok(account);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCurrentAccountDto dto)
        {
            if (!await HasAccessToCompany(dto.CompanyId))
                throw new BusinessException(ErrorKeys.Unauthorized);

            await _currentAccountService.AddAsync(dto);
            _logger.LogInformation("Current account created for CompanyId: {CompanyId}", dto.CompanyId);
            return Ok(new { Message = "Current account created successfully." });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateCurrentAccountDto dto)
        {
            if (!await HasAccessToCompany(dto.CompanyId))
                throw new BusinessException(ErrorKeys.Unauthorized);

            await _currentAccountService.UpdateAsync(dto);
            _logger.LogInformation("Current account updated. Id: {AccountId}", dto.Id);
            return Ok(new { Message = "Current account updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var account = await _currentAccountService.GetByIdAsync(id);
            if (!await HasAccessToCompany(account.CompanyId))
                throw new BusinessException(ErrorKeys.Unauthorized);

            await _currentAccountService.DeleteAsync(id);
            _logger.LogInformation("Current account deleted. Id: {AccountId}", id);
            return Ok(new { Message = "Current account deleted successfully." });
        }
    }
}