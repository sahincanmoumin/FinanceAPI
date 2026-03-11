using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.DTOs.InvoiceDetail
{
    public class UpdateInvoiceDetailDto
    {
        public int Id { get; set; }
        public int StockId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}

