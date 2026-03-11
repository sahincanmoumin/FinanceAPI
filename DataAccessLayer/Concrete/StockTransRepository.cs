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
    public class StockTransRepository : GenericRepository<StockTrans>, IStockTransRepository
    {
        private readonly AppDbContext _context;

        public StockTransRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
