using BusinessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.Pagination;
using EntityLayer.DTOs.Stock;
using EntityLayer.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiLayer.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StocksController : BaseController
    {
        private readonly IStockService _stockService;
        private readonly ICompanyService _companyService;
        private readonly ILogger<StocksController> _logger;

        public StocksController(IStockService stockService, ICompanyService companyService, ILogger<StocksController> logger)
        {
            _stockService = stockService;
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
        public async Task<IActionResult> GetAll([FromQuery] StockFilterDto filter, [FromQuery] int companyId)
        {
            if (!await HasAccessToCompany(companyId))
                throw new BusinessException(ErrorKeys.Unauthorized);

            var result = await _stockService.GetAllStocksAsync(filter, companyId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var stock = await _stockService.GetByIdAsync(id);
            if (!await HasAccessToCompany(stock.CompanyId))
                throw new BusinessException(ErrorKeys.Unauthorized);

            return Ok(stock);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStockDto dto)
        {
            if (!await HasAccessToCompany(dto.CompanyId))
                throw new BusinessException(ErrorKeys.Unauthorized);

            await _stockService.AddAsync(dto);
            _logger.LogInformation("Stock created for CompanyId: {CompanyId}", dto.CompanyId);
            return Ok(new { Message = "Stock created successfully." });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateStockDto dto)
        {
            if (!await HasAccessToCompany(dto.CompanyId))
                throw new BusinessException(ErrorKeys.Unauthorized);

            await _stockService.UpdateAsync(dto);
            _logger.LogInformation("Stock updated. StockId: {StockId}", dto.Id);
            return Ok(new { Message = "Stock updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var stock = await _stockService.GetByIdAsync(id);
            if (!await HasAccessToCompany(stock.CompanyId))
                throw new BusinessException(ErrorKeys.Unauthorized);

            await _stockService.DeleteAsync(id);
            _logger.LogInformation("Stock deleted. StockId: {StockId}", id);
            return Ok(new { Message = "Stock deleted successfully." });
        }
    }
}