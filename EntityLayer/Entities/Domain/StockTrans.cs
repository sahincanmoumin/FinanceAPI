using EntityLayer.Entities.Common;
using EntityLayer.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.Entities.Domain
{
    public class StockTrans : BaseEntity
    {
        public DateTime Date { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public TransactionType Direction { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; }

        public int StockId { get; set; }
        public Stock Stock { get; set; }

    }
}