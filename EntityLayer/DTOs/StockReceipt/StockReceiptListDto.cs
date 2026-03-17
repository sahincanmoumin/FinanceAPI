using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityLayer.Entities.Enums;
using EntityLayer.DTOs.StockReceiptDetail;

namespace EntityLayer.DTOs.StockReceipt
{
    public class StockReceiptListDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }

        public int WarehouseId { get; set; }

        public int? TargetWarehouseId { get; set; }

        public int? CurrentAccountId { get; set; }

        public string SerialNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public ReceiptStatus Status { get; set; } 
        public ReceiptType Type { get; set; }

        public List<StockReceiptDetailListDto> Details { get; set; }

    }
}
