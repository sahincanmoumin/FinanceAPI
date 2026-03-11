using AutoMapper;
using BusinessLayer.Abstract;
using DataAccessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.Pagination;
using EntityLayer.DTOs.Role;
using EntityLayer.Entities.Domain;
using EntityLayer.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLayer.Concrete
{
    public class UserRoleService : IUserRoleService
    {
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;

        public UserRoleService(IUserRoleRepository userRoleRepository, IRoleRepository roleRepository, IMapper mapper)
        {
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
            _mapper = mapper;
        }

        public async Task<PagedResponse<UserRoleListDto>> GetRolesByUserIdAsync(int userId, UserRoleFilterDto filter)
        {
            var validFilter = new UserRoleFilterDto(filter.PageNumber, filter.PageSize);

            var query = _userRoleRepository.GetQueryable()
                .AsNoTracking()
                .Include(x => x.Role)
                .Where(x => x.UserId == userId);

            if (!string.IsNullOrWhiteSpace(filter.RoleName))
            {
                query = query.Where(x => x.Role.Name.Contains(filter.RoleName));
            }

            var totalRecords = await query.CountAsync();

            var userRoles = await query
                .OrderByDescending(x => x.CreateDate)
                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                .Take(validFilter.PageSize)
                .ToListAsync();

            var mappedData = _mapper.Map<IEnumerable<UserRoleListDto>>(userRoles);

            return new PagedResponse<UserRoleListDto>(mappedData, totalRecords, validFilter.PageNumber, validFilter.PageSize);
        }

        public async Task AssignRoleToUserAsync(CreateUserRoleDto dto)
        {
            var role = await _roleRepository.GetByIdAsync(dto.RoleId);
            if (role == null) throw new BusinessException(ErrorKeys.RoleNotFound);

            var isExist = await _userRoleRepository.HasRoleAsync(dto.UserId, role.Name); 
            if (isExist) throw new BusinessException(ErrorKeys.RoleAlreadyExists);

            var userRole = _mapper.Map<UserRole>(dto);
            await _userRoleRepository.AddAsync(userRole);
        }

        public async Task RemoveRoleFromUserAsync(int userId, int roleId)
        {
            var userRole = await _userRoleRepository.GetQueryable()
                .FirstOrDefaultAsync(x => x.UserId == userId && x.RoleId == roleId);    

            if (userRole == null) throw new BusinessException(ErrorKeys.UserRoleNotFound);

            _userRoleRepository.Delete(userRole);
        }
    }
}