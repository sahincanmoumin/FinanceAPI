using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityLayer.Entities.Enums;

namespace EntityLayer.DTOs.StockTrans
{
    public class StockTransListDto
    {
        public int Id { get; set; }
        public int StockId { get; set; }
        public DateTime Date { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public TransactionType Direction { get; set; }
    }
}