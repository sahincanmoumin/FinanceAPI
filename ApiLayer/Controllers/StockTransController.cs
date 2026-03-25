using BusinessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.Pagination;
using EntityLayer.DTOs.StockTrans;
using EntityLayer.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ApiLayer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class StockTransController : BaseController
    {
        private readonly IStockTransService _stockTransService;
        private readonly ICompanyService _companyService;

        public StockTransController(
            IStockTransService stockTransService,
            ICompanyService companyService)
        {
            _stockTransService = stockTransService;
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
        public async Task<IActionResult> GetAll([FromQuery] StockTransFilterDto filter)
        {
            if (filter.CompanyId <= 0)
                throw new BusinessException(ErrorKeys.CompanyIdRequired);

            if (!await HasAccessToCompany(filter.CompanyId))
                throw new BusinessException(ErrorKeys.Unauthorized); 

            var result = await _stockTransService.GetAllTransactionsAsync(filter);
            return Ok(result);
        }
    }
}