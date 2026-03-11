using EntityLayer.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityLayer.Entities.Auth;

namespace EntityLayer.Entities.Domain
{
    public class Company : BaseEntity
    {
        public string Name { get; set; }
        public string TaxNumber { get; set; }
        public string TaxOffice { get; set; }
        public string Address { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
        public ICollection<Stock> Stocks { get; set; }
        public ICollection<CurrentAccount> CurrentAccounts { get; set; }
        public ICollection<Invoice> Invoices { get; set; }

    }
}
