using EntityLayer.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.Entities.Domain
{
    public class StockReceiptDetail : BaseEntity
    {
        public int StockReceiptId { get; set; }
        public int StockId { get; set; }

        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public StockReceipt StockReceipt { get; set; }
        public Stock Stock { get; set; }
    }
}