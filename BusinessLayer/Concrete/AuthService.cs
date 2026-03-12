using BCrypt.Net;
using BusinessLayer.Abstract;
using DataAccessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.Auth;
using EntityLayer.Entities.Auth;
using EntityLayer.Entities.Domain;
using EntityLayer.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Concrete
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IConfiguration _configuration;
        private readonly IValidator<RegisterDto> _registerValidator;
        private readonly IValidator<LoginDto> _loginValidator;

        public AuthService(
            IUserRepository userRepository,
            IUserRoleRepository userRoleRepository,
            IRoleRepository roleRepository,
            IConfiguration configuration,
            IValidator<RegisterDto> registerValidator,
            IValidator<LoginDto> loginValidator)
        {
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
            _configuration = configuration;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
        }

        // --- PRIVATE VALIDATION ---
        private async Task ValidateForRegisterAsync(RegisterDto dto)
        {
            var validationResult = await _registerValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new BusinessException(errors);
            }
        }

        private async Task ValidateForLoginAsync(LoginDto dto)
        {
            var validationResult = await _loginValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new BusinessException(errors);
            }
        }

        // --- ANA METOTLAR ---
        public async Task RegisterAsync(RegisterDto dto)
        {
            await ValidateForRegisterAsync(dto);

            var user = new User
            {
                UserName = dto.UserName,
                FullName = dto.FullName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            await _userRepository.AddAsync(user);

            var defaultRole = await _roleRepository.GetQueryable()
                                                   .FirstOrDefaultAsync(x => x.Name == "User");

            if (defaultRole != null)
            {
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = defaultRole.Id
                };
                await _userRoleRepository.AddAsync(userRole);
            }
            else
            {
                throw new BusinessException(ErrorKeys.RoleNotFound);
            }
            await _userRepository.SaveChangesAsync();
        }

        public async Task<string> LoginAsync(LoginDto dto)
        {
            await ValidateForLoginAsync(dto);

            var user = await _userRepository.GetQueryable()
                                            .FirstOrDefaultAsync(x => x.UserName == dto.UserName);

            if (user == null) throw new BusinessException(ErrorKeys.UserNotFound);

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!isPasswordValid) throw new BusinessException(ErrorKeys.WrongPassword);

            var userRoles = await _userRoleRepository.GetQueryable()
                                                     .Where(x => x.UserId == user.Id)
                                                     .ToListAsync();

            var roles = new List<string>();
            foreach (var ur in userRoles)
            {
                var role = await _roleRepository.GetByIdAsync(ur.RoleId);
                if (role != null) roles.Add(role.Name);
            }

            return GenerateJwtToken(user, roles);
        }

        private string GenerateJwtToken(User user, List<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}