using DataAccessLayer.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer.Context;
using EntityLayer.Entities.Domain;

namespace DataAccessLayer.Concrete
{
    public class InvoiceDetailRepository : GenericRepository<InvoiceDetail>, IInvoiceDetailRepository
    {
        private readonly AppDbContext _context;

        public InvoiceDetailRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
