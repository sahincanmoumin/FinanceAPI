using EntityLayer.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityLayer.DTOs.StockReceiptDetail;

namespace EntityLayer.DTOs.StockReceipt
{
    public class CreateStockReceiptDto
    {
        public int CompanyId { get; set; }
        public int WarehouseId { get; set; }
        public int? TargetWarehouseId { get; set; }
        public int? CurrentAccountId { get; set; }
        public string SerialNumber { get; set; }
        public ReceiptType Type { get; set; }
        public List<CreateStockReceiptDetailDto> Details { get; set; }
    }
}
