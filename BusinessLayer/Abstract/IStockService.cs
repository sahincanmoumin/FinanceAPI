using EntityLayer.DTOs.Pagination;
using EntityLayer.DTOs.Stock;
using System.Threading.Tasks;

namespace BusinessLayer.Abstract
{
    public interface IStockService
    {
        Task<PagedResponse<StockListDto>> GetAllStocksAsync(StockFilterDto filter, int companyId);
        Task<StockListDto> GetByIdAsync(int id);
        Task AddAsync(CreateStockDto dto);
        Task UpdateAsync(UpdateStockDto dto);
        Task DeleteAsync(int id);
    }
}