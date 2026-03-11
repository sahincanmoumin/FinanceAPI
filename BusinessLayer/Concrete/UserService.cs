using AutoMapper;
using BusinessLayer.Abstract;
using DataAccessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.Auth;
using EntityLayer.DTOs.Pagination;
using EntityLayer.Entities.Auth;
using EntityLayer.Exceptions;
using EntityLayer.Utilities;
using EntityLayer.DTOs.User;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLayer.Concrete
{
    public class UserService : IUserService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IMapper _mapper;

        public UserService(IGenericRepository<User> userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<PagedResponse<UserListDto>> GetAllUserAsync(UserFilterDto filter)
        {
            var validFilter = new UserFilterDto(filter.PageNumber, filter.PageSize);

            var query = _userRepository.GetQueryable()
                                       .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                var term = filter.Name.ToLower();
                query = query.Where(u => u.UserName.ToLower().Contains(term) ||
                                         u.FullName.ToLower().Contains(term));
            }

            var totalRecords = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.CreateDate)
                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                .Take(validFilter.PageSize)
                .ToListAsync();

            var mappedUsers = _mapper.Map<IEnumerable<UserListDto>>(users);

            return new PagedResponse<UserListDto>(mappedUsers, totalRecords, validFilter.PageNumber, validFilter.PageSize);
        }

        public async Task<User> GetByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) throw new BusinessException(ErrorKeys.UserNotFound);
            return user;
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await GetByIdAsync(id);
            if (user == null) throw new BusinessException(ErrorKeys.UserNotFound);
            _userRepository.Delete(user);
        }
        public async Task UpdateAsync(UserUpdateDto dto)
        {
            var user = await _userRepository.GetByIdAsync(dto.Id);
            if (user == null) throw new BusinessException(ErrorKeys.UserNotFound);

            if (user.UserName != dto.UserName)
            {
                var isExist = await _userRepository.AnyAsync(x => x.UserName == dto.UserName);
                if (isExist) throw new BusinessException(ErrorKeys.UserNameAlreadyExists);
            }

            user.FullName = dto.FullName;
            user.UserName = dto.UserName;

            // 3. Şifre alanı dolu geldiyse şifreyi değiştir (Sadeleştirilmiş mantık)
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.PasswordHash = HashingHelper.HashPassword(dto.Password);
            }

            _userRepository.Update(user);
        }
    }
}