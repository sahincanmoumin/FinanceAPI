using EntityLayer.Entities.Auth;    
using EntityLayer.Entities.Common;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
namespace DataAccessLayer.Abstract
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User> GetByUsernameAsync(string username);
        Task<List<User>> GetAllAsync();

    }
}
