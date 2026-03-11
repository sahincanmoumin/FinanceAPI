using EntityLayer.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EntityLayer.Entities.Domain
{
    public class InvoiceDetail : BaseEntity
    {
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }


        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; }
        public int StockId { get; set; }
        public Stock Stock { get; set; }
    }
}