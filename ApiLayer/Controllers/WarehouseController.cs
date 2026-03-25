using BusinessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.Pagination;
using EntityLayer.DTOs.Warehouse;
using EntityLayer.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ApiLayer.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController : BaseController
    {
        private readonly IWarehouseService _warehouseService;
        private readonly ICompanyService _companyService;

        public WarehouseController(IWarehouseService warehouseService, ICompanyService companyService)
        {
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

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] WarehouseFilterDto filter, [FromQuery] int companyId)
        {
            if (!await HasAccessToCompany(companyId))   
                throw new BusinessException(ErrorKeys.Unauthorized);

            
            var result = await _warehouseService.GetAllAsync(filter);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _warehouseService.GetByIdAsync(id);
            if (!await HasAccessToCompany(result.CompanyId))
                throw new BusinessException(ErrorKeys.Unauthorized);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateWarehouseDto dto)
        {
            if (!await HasAccessToCompany(dto.CompanyId))
                throw new BusinessException(ErrorKeys.Unauthorized);

            var result = await _warehouseService.AddAsync(dto);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateWarehouseDto dto)
        {
            var warehouse = await _warehouseService.GetByIdAsync(dto.Id);
            if (!await HasAccessToCompany(warehouse.CompanyId))
                throw new BusinessException(ErrorKeys.Unauthorized);

            var result = await _warehouseService.UpdateAsync(dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var warehouse = await _warehouseService.GetByIdAsync(id);
            if (!await HasAccessToCompany(warehouse.CompanyId))
                throw new BusinessException(ErrorKeys.Unauthorized);

            await _warehouseService.DeleteAsync(id);
            return Ok(new { Message = "Depo başarıyla silindi." });
        }
    }
}