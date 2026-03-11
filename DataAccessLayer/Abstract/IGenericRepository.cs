using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EntityLayer.Entities.Common;

namespace DataAccessLayer.Abstract
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        IQueryable<T> GetQueryable();
        IQueryable<T> Where(Expression<Func<T, bool>> expression);
        Task<T> GetByIdAsync(int id);
        Task<bool> AnyAsync(Expression<Func<T, bool>> expression);
        Task AddAsync(T entity);
        void Update(T entity); 
        void Delete(T entity); 
        Task<int> SaveChangesAsync();
    }
}