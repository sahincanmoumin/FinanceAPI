using BusinessLayer.Abstract;
using EntityLayer.Constants;
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
        private readonly IWarehouseService _warehouseService;
        private readonly ICompanyService _companyService;

        public StockWarehouseController(
            IStockWarehouseService stockWarehouseService,
            IWarehouseService warehouseService,
            ICompanyService companyService)
        {
            _stockWarehouseService = stockWarehouseService;
            _warehouseService = warehouseService;
            _companyService = companyService;
        }

        private async Task<bool> HasAccessToCompany(int companyId)
        {
            if (User.IsInRole("Admin")) return true;
            var company = await _companyService.GetByIdAsync(companyId);
            int loggedInUserId = GetUserId();
            return company != null && company.UserId == loggedInUserId;
        }

        [HttpGet("warehouse/{warehouseId}")]
        public async Task<IActionResult> GetStockStatusByWarehouse(int warehouseId)
        {
            var warehouse = await _warehouseService.GetByIdAsync(warehouseId);

            if (!await HasAccessToCompany(warehouse.CompanyId))
                throw new BusinessException(ErrorKeys.Unauthorized);

            var result = await _stockWarehouseService.GetStockStatusByWarehouseAsync(warehouseId);
            return Ok(result);
        }
    }
}