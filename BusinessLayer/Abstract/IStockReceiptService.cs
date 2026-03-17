using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityLayer.DTOs.Pagination;
using EntityLayer.DTOs.StockReceipt;

namespace BusinessLayer.Abstract
{
    public interface IStockReceiptService
    {
        Task<PagedResponse<StockReceiptListDto>> GetAllAsync(StockReceiptFilterDto filter);

        Task<StockReceiptListDto> GetByIdAsync(int id);

        Task<StockReceiptListDto> AddAsync(CreateStockReceiptDto dto);
        Task<StockReceiptListDto> UpdateAsync(UpdateStockReceiptDto dto);
        Task DeleteAsync(int id);

        Task ApproveAsync(int id, bool isNested = false);
    }
}