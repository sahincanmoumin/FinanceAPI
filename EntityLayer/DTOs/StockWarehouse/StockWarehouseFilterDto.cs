using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.DTOs.StockWarehouse
{
    public class StockWarehouseFilterDto
    {
        public int CompanyId { get; set; }
        public int? WarehouseId { get; set; }
        public int? StockId { get; set; }
    }
}