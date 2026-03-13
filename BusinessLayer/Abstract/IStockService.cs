using EntityLayer.DTOs.Pagination;
using EntityLayer.DTOs.Stock;
using System.Threading.Tasks;

namespace BusinessLayer.Abstract
{
    public interface IStockService
    {
        Task<PagedResponse<StockListDto>> GetAllStocksAsync(StockFilterDto filter, int companyId);
        Task<StockListDto> GetByIdAsync(int id);
        Task<StockListDto> AddAsync(CreateStockDto dto);
        Task<StockListDto> UpdateAsync(UpdateStockDto dto);
        Task DeleteAsync(int id);
    }
}