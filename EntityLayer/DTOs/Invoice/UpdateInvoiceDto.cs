using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.DTOs.Invoice
{
    public class UpdateInvoiceDto
    {
        public int Id { get; set; }
        public int CurrentAccountId { get; set; }
        public string SerialNumber { get; set; }
        public Guid? Ettn { get; set; }
        public DateTime Date { get; set; }
    }
}