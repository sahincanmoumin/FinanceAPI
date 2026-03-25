using EntityLayer.Entities.Common;
using EntityLayer.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.Entities.Domain
{
    public class Warehouse : BaseEntity 
    {
        public int CompanyId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public WarehouseType Type { get; set; }

        public Company Company { get; set; }
        public ICollection<StockReceipt> StockReceipts { get; set; }
        public ICollection<StockWarehouse> StockWarehouses { get; set; }
    }
}   