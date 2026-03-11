using DataAccessLayer.Abstract;
using DataAccessLayer.Context;
using EntityLayer.Entities.Common;
using EntityLayer.Entities.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Concrete
{
    public class CurrentAccountRepository : GenericRepository<CurrentAccount>, ICurrentAccountRepository
    {
        private readonly AppDbContext _context;

        public CurrentAccountRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
