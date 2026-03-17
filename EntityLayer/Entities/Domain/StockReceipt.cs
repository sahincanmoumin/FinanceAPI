using EntityLayer.Entities.Common;
using EntityLayer.Entities.Enums;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.Entities.Domain
{
    public class StockReceipt : BaseEntity
    {
        public int CompanyId { get; set; }

        public int WarehouseId { get; set; }

        public int? TargetWarehouseId { get; set; }

        public int? CurrentAccountId { get; set; }

        public string SerialNumber { get; set; }

        public ReceiptStatus Status { get; set; } // Draft//Approved
        public ReceiptType Type { get; set; } // Input//Output//Transfer

      
        public Company Company { get; set; }
        public Warehouse Warehouse { get; set; }
        public Warehouse TargetWarehouse { get; set; }
        public CurrentAccount CurrentAccount { get; set; }

        public ICollection<StockReceiptDetail> Details { get; set; }
    }
}