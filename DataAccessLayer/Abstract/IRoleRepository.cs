using EntityLayer.Entities.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Abstract
{
    public interface IRoleRepository: IGenericRepository<Role>
    {
        Task<Role?> GetByNameAsync(string roleName);
    }

}
