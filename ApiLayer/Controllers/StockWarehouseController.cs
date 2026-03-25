using BusinessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.StockWarehouse;
using EntityLayer.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ApiLayer.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StockWarehouseController : BaseController
    {
        private readonly IStockWarehouseService _stockWarehouseService;
        private readonly ICompanyService _companyService;

        public StockWarehouseController(
            IStockWarehouseService stockWarehouseService,
            ICompanyService companyService)
        {
            _stockWarehouseService = stockWarehouseService;
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
        public async Task<IActionResult> GetAll([FromQuery] StockWarehouseFilterDto filter)
        {
            if (filter.CompanyId <= 0)
                throw new BusinessException(ErrorKeys.CompanyIdRequired);

            if (!await HasAccessToCompany(filter.CompanyId))
                throw new BusinessException(ErrorKeys.Unauthorized);

            var result = await _stockWarehouseService.GetAllStockStatusAsync(filter);
            return Ok(result);
        }
    }
}