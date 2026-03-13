using EntityLayer.DTOs.Pagination;
using EntityLayer.DTOs.StockTrans;
using System.Threading.Tasks;
using EntityLayer.Entities.Enums;

namespace BusinessLayer.Abstract
{
    public interface IStockTransService
    {
        Task ProcessStockActionAsync(int companyId, int stockId, decimal quantity, decimal unitPrice, TransactionType direction);
        Task<PagedResponse<StockTransListDto>> GetTransactionsByStockIdAsync(int stockId, StockTransFilterDto filter);
    }
}