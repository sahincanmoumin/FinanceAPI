using EntityLayer.DTOs.Pagination;
using EntityLayer.DTOs.Role;
using System.Threading.Tasks;

namespace BusinessLayer.Abstract
{
    public interface IRoleService
    {   
        Task<PagedResponse<RoleListDto>> GetAllRolesAsync(RoleFilterDto filter);
        Task<RoleListDto> GetByIdAsync(int id);
        Task AddAsync(CreateRoleDto dto);
        Task UpdateAsync(UpdateRoleDto dto);
        Task DeleteAsync(int id);
    }
}