using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityLayer.DTOs.Pagination;
using EntityLayer.DTOs.Role;
using System.Threading.Tasks;

namespace BusinessLayer.Abstract
{
    public interface IUserRoleService
    {
        Task<PagedResponse<UserRoleListDto>> GetRolesByUserIdAsync(int userId, UserRoleFilterDto filter);
        Task AssignRoleToUserAsync(CreateUserRoleDto dto);
        Task RemoveRoleFromUserAsync(int userId, int roleId);
    }
}