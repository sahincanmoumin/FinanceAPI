using EntityLayer.Entities.Common;
using EntityLayer.Entities.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.Entities
{
    public class StockWarehouse : BaseEntity
    {
        public int StockId { get; set; }
        public Stock Stock { get; set; }

        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }
        public decimal Quantity { get; set; }
    }
}