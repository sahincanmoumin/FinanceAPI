using EntityLayer.Entities.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Abstract
{
    public interface IUserRoleRepository : IGenericRepository<UserRole>
    {
        Task<List<string>> GetUserRolesAsync(int userId);
        Task<bool> HasRoleAsync(int userId, string roleName);
    }
}
