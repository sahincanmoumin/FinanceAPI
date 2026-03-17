using EntityLayer.DTOs.Pagination;
using EntityLayer.DTOs.Warehouse;
using System.Threading.Tasks;

namespace BusinessLayer.Abstract
{
    public interface IWarehouseService
    {
        Task<PagedResponse<WarehouseListDto>> GetAllAsync(WarehouseFilterDto filter);
        Task<WarehouseListDto> GetByIdAsync(int id);
        Task<WarehouseListDto> AddAsync(CreateWarehouseDto dto);
        Task<WarehouseListDto> UpdateAsync(UpdateWarehouseDto dto);
        Task DeleteAsync(int id);
    }
}