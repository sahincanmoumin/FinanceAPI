using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using EntityLayer.Entities.Enums;
using EntityLayer.DTOs.InvoiceDetail;

namespace EntityLayer.DTOs.Invoice
{
    public class CreateInvoiceDto
    {
        public int CompanyId { get; set; }
        public int CurrentAccountId { get; set; }
        public string SerialNumber { get; set; }
        public Guid? Ettn { get; set; }
        public DateTime Date { get; set; }
        public InvoiceStatus Status { get; set; } 

        public List<CreateInvoiceDetailDto> InvoiceDetails { get; set; }
    }
}
