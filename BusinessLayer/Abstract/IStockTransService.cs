using EntityLayer.DTOs.Pagination;
using EntityLayer.DTOs.StockTrans;
using System.Threading.Tasks;

namespace BusinessLayer.Abstract
{
    public interface IStockTransService
    {
        Task<PagedResponse<StockTransListDto>> GetTransactionsByStockIdAsync(int stockId, StockTransFilterDto filter);
    }
}