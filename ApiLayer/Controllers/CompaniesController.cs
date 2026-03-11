using BusinessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.Company;
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
    public class CompaniesController : BaseController
    {
        private readonly ICompanyService _companyService;
        private readonly ILogger<CompaniesController> _logger;

        public CompaniesController(ICompanyService companyService, ILogger<CompaniesController> logger)
        {
            _companyService = companyService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] CompanyFilterDto filter)
        {
            int queryUserId = IsAdmin() ? 0 : GetUserId();
            var result = await _companyService.GetAllCompaniesAsync(filter, queryUserId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var company = await _companyService.GetByIdAsync(id);
            if (!IsAdmin() && company.UserId != GetUserId()) 
                throw new BusinessException(ErrorKeys.Unauthorized);

            return Ok(company);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCompanyDto dto)
        {
            if (!IsAdmin()) dto.UserId = GetUserId();

            await _companyService.AddAsync(dto);
            _logger.LogInformation("Company created. Name: {CompanyName}", dto.Name);
            return Ok(new { Message = "Company created successfully." });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateCompanyDto dto)
        {
            var existingCompany = await _companyService.GetByIdAsync(dto.Id);
            if (!IsAdmin() && existingCompany.UserId != GetUserId())
                throw new BusinessException(ErrorKeys.Unauthorized);

            if (!IsAdmin() && dto.UserId != GetUserId())
                throw new BusinessException(ErrorKeys.Unauthorized);


            await _companyService.UpdateAsync(dto);
            _logger.LogInformation("Company updated. Id: {CompanyId}", dto.Id);
            return Ok(new { Message = "Company updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var company = await _companyService.GetByIdAsync(id);

            if (!IsAdmin() && company.UserId != GetUserId())
                throw new BusinessException(ErrorKeys.Unauthorized);

            await _companyService.DeleteAsync(id);
            _logger.LogInformation("Company deleted. Id: {CompanyId}", id);
            return Ok(new { Message = "Company deleted successfully." });
        }
    }
}