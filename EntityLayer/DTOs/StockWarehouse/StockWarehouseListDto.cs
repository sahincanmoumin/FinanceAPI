using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.DTOs.StockWarehouse
{
    public class StockWarehouseListDto
    {
        public int Id { get; set; }
        public int StockId { get; set; }
        public string StockName { get; set; }
        public string StockCode { get; set; } 
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } 
        public decimal Quantity { get; set; } 
    }
}
