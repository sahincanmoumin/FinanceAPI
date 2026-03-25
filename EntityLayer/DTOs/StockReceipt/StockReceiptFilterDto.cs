using EntityLayer.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityLayer.DTOs.Pagination;

namespace EntityLayer.DTOs.StockReceipt
{
    public class StockReceiptFilterDto : PaginationFilter
    {
        public StockReceiptFilterDto() : base()
        {
        }

        public StockReceiptFilterDto(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public int? CompanyId { get; set; }
        public int? WarehouseId { get; set; }
        public string? SerialNumber { get; set; }
        public ReceiptStatus? Status { get; set; }
        public ReceiptType? Type { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}