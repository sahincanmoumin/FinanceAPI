using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityLayer.Entities.Enums;
using EntityLayer.DTOs.InvoiceDetail;


namespace EntityLayer.DTOs.Invoice
{
    public class InvoiceListDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string SerialNumber { get; set; }
        public Guid? Ettn { get; set; }
        public DateTime Date { get; set; }
        public InvoiceStatus Status { get; set; }
        public InvoiceType Type { get; set; }
        public int CurrentAccountId { get; set; }
        public List<InvoiceDetailListDto> InvoiceDetails { get; set; }
    }
}
