using AutoMapper;
using BusinessLayer.Abstract;
using DataAccessLayer.Abstract;         
using EntityLayer.Constants;
using EntityLayer.DTOs.Pagination;
using EntityLayer.DTOs.Role;
using EntityLayer.DTOs.User;
using EntityLayer.Entities.Auth;
using EntityLayer.Entities.Domain;
using EntityLayer.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLayer.Concrete
{
    public class RoleService : IRoleService
    {
        private readonly IGenericRepository<UserRole> _userRoleRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Role> _roleRepository;
        private readonly IMapper _mapper;


        public RoleService(
            IGenericRepository<UserRole> userRoleRepository,
            IGenericRepository<User> userRepository,
            IGenericRepository<Role> roleRepository,
            IMapper mapper)
        {
            _userRoleRepository = userRoleRepository;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _mapper = mapper;
        }
        public async Task AssignRoleAsync(AssignRoleDto dto)
        {
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null) throw new BusinessException(ErrorKeys.UserNotFound);

            var role = await _roleRepository.GetByIdAsync(dto.RoleId);
            if (role == null) throw new BusinessException(ErrorKeys.RoleNotFound);

            var exists = await _userRoleRepository.GetQueryable()
                .AnyAsync(x => x.UserId == dto.UserId && x.RoleId == dto.RoleId);

            if (exists) throw new BusinessException(ErrorKeys.UserAlreadyHasRole);

            var userRole = new UserRole
            {
                UserId = dto.UserId,
                RoleId = dto.RoleId
            };

            await _userRoleRepository.AddAsync(userRole);
        }

        public async Task<PagedResponse<RoleListDto>> GetAllRolesAsync(RoleFilterDto filter)
        {
            var validFilter = new RoleFilterDto(filter.PageNumber, filter.PageSize);
            var query = _roleRepository.GetQueryable()
                                       .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                query = query.Where(x => x.Name.Contains(filter.Name));
            }

            var totalRecords = await query.CountAsync();

            var roles = await query
                .OrderByDescending(x => x.CreateDate)
                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                .Take(validFilter.PageSize)
                .ToListAsync();

            var mappedRoles = _mapper.Map<IEnumerable<RoleListDto>>(roles);

            return new PagedResponse<RoleListDto>(mappedRoles, totalRecords, validFilter.PageNumber, validFilter.PageSize);
        }

        public async Task<RoleListDto> GetByIdAsync(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null) throw new BusinessException(ErrorKeys.RoleNotFound);

            return _mapper.Map<RoleListDto>(role);
        }

        public async Task AddAsync(CreateRoleDto dto)
        {
            var isExist = await _roleRepository.AnyAsync(x => x.Name == dto.Name);
            if (isExist) throw new BusinessException(ErrorKeys.RoleAlreadyExists);

            var role = _mapper.Map<Role>(dto);
            await _roleRepository.AddAsync(role);
        }

        public async Task UpdateAsync(UpdateRoleDto dto)
        {
            var role = await _roleRepository.GetByIdAsync(dto.Id);
            if (role == null) throw new BusinessException(ErrorKeys.RoleNotFound);

            _mapper.Map(dto, role);
            _roleRepository.Update(role);
        }

        public async Task DeleteAsync(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null) throw new BusinessException(ErrorKeys.RoleNotFound);

            _roleRepository.Delete(role);
        }
    }
}