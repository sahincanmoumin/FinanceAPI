using EntityLayer.Entities.Common;
using EntityLayer.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EntityLayer.Entities.Domain
{
    public class Invoice : BaseEntity
    {

        public string SerialNumber { get; set; }
        public Guid? Ettn { get; set; } 

        public InvoiceStatus Status { get; set; }
        public InvoiceType Type { get; set; }
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }
        public int CompanyId { get; set; }
        public Company Company { get; set; }
        public int CurrentAccountId { get; set; }
        public CurrentAccount CurrentAccount { get; set; }
        public ICollection<InvoiceDetail> InvoiceDetails { get; set; }
    }
}