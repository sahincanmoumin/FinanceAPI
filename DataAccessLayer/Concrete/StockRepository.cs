using DataAccessLayer.Abstract;
using DataAccessLayer.Context;
using EntityLayer.Entities.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Concrete
{
    public class StockRepository : GenericRepository<Stock>, IStockRepository
    {
        private readonly AppDbContext _context;

        public StockRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
