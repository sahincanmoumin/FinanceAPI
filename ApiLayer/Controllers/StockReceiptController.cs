using BusinessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.Pagination;
using EntityLayer.DTOs.StockReceipt;
using EntityLayer.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ApiLayer.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StockReceiptController : BaseController
    {
        private readonly IStockReceiptService _stockReceiptService;
        private readonly ICompanyService _companyService;

        public StockReceiptController(IStockReceiptService stockReceiptService, ICompanyService companyService)
        {
            _stockReceiptService = stockReceiptService;
            _companyService = companyService;
        }

        private async Task<bool> HasAccessToCompany(int companyId)
        {
            if (User.IsInRole("Admin")) return true;
            var company = await _companyService.GetByIdAsync(companyId);
            int loggedInUserId = GetUserId();
            return company != null && company.UserId == loggedInUserId;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] StockReceiptFilterDto filter, [FromQuery] int companyId)
        {
            if (!await HasAccessToCompany(companyId))
                throw new BusinessException(ErrorKeys.Unauthorized);

            var result = await _stockReceiptService.GetAllAsync(filter);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _stockReceiptService.GetByIdAsync(id);
            if (!await HasAccessToCompany(result.CompanyId))
                throw new BusinessException(ErrorKeys.Unauthorized);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStockReceiptDto dto)
        {
            if (!await HasAccessToCompany(dto.CompanyId))
                throw new BusinessException(ErrorKeys.Unauthorized);

            var result = await _stockReceiptService.AddAsync(dto);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateStockReceiptDto dto)
        {
            var receipt = await _stockReceiptService.GetByIdAsync(dto.Id);
            if (!await HasAccessToCompany(receipt.CompanyId))
                throw new BusinessException(ErrorKeys.Unauthorized);

            var result = await _stockReceiptService.UpdateAsync(dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var receipt = await _stockReceiptService.GetByIdAsync(id);
            if (!await HasAccessToCompany(receipt.CompanyId))
                throw new BusinessException(ErrorKeys.Unauthorized);

            await _stockReceiptService.DeleteAsync(id);
            return Ok(new { Message = "Fiş başarıyla silindi." });
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var receipt = await _stockReceiptService.GetByIdAsync(id);
            if (!await HasAccessToCompany(receipt.CompanyId))
                throw new BusinessException(ErrorKeys.Unauthorized);

            await _stockReceiptService.ApproveAsync(id);
            return Ok(new { Message = "Fiş başarıyla onaylandı." });
        }
    }
}