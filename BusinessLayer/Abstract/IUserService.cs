using EntityLayer.DTOs.Auth;
using EntityLayer.DTOs.User;
using EntityLayer.Entities.Auth;
using EntityLayer.DTOs.Pagination;
using System.Threading.Tasks;

namespace BusinessLayer.Abstract
{
    public interface IUserService
    {
        Task<PagedResponse<UserListDto>> GetAllUserAsync(UserFilterDto filter);
        Task<User> GetByIdAsync(int id); 
        Task DeleteUserAsync(int id);
        Task UpdateAsync(UserUpdateDto dto);
    }
}