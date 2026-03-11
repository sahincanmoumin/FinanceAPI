using BusinessLayer.Concrete;
using DataAccessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.Auth;
using EntityLayer.Entities.Auth;
using EntityLayer.Entities.Domain;
using EntityLayer.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FinanceApi.Tests.AuthTest
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IUserRoleRepository> _mockUserRoleRepo;
        private readonly Mock<IRoleRepository> _mockRoleRepo;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _mockUserRoleRepo = new Mock<IUserRoleRepository>();
            _mockRoleRepo = new Mock<IRoleRepository>();
            _mockConfig = new Mock<IConfiguration>();

            _mockConfig.Setup(x => x["Jwt:SecretKey"]).Returns("TestIcinCokGizliBirAnahtar1234567890123456");
            _mockConfig.Setup(x => x["Jwt:Issuer"]).Returns("FinanceApi");
            _mockConfig.Setup(x => x["Jwt:Audience"]).Returns("FinanceApiUser");

            _authService = new AuthService(
                _mockUserRepo.Object,
                _mockUserRoleRepo.Object,
                _mockRoleRepo.Object,
                _mockConfig.Object
            );
        }

        [Fact]
        public async Task Register_WhenRoleExists_ShouldCreateUser()
        {
            var dto = new RegisterDto { UserName = "sahin", Password = "123", FullName = "Sahin Test" };
            var rolesList = new List<Role> { new Role { Id = 1, Name = "User" } };

            var mockRoles = rolesList.BuildMock();
            _mockRoleRepo.Setup(x => x.GetQueryable()).Returns(mockRoles);

            await _authService.RegisterAsync(dto);

            _mockUserRepo.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
            _mockUserRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Register_WhenRoleNotFound_ShouldThrowException()
        {
            var dto = new RegisterDto { UserName = "user", Password = "123" };
            var emptyRoles = new List<Role>().BuildMock();
            _mockRoleRepo.Setup(x => x.GetQueryable()).Returns(emptyRoles);

            await _authService.Invoking(s => s.RegisterAsync(dto))
                .Should().ThrowAsync<BusinessException>()
                .WithMessage(ErrorKeys.RoleNotFound);
        }

        [Fact]
        public async Task Login_WhenCredentialsAreValid_ShouldReturnToken()
        {
            var password = "123";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var dbUser = new User { Id = 1, UserName = "sahin", PasswordHash = hashedPassword };

            var usersMock = new List<User> { dbUser }.BuildMock();
            var userRolesMock = new List<UserRole> { new UserRole { UserId = 1, RoleId = 10 } }.BuildMock();

            _mockUserRepo.Setup(x => x.GetQueryable()).Returns(usersMock);
            _mockUserRoleRepo.Setup(x => x.GetQueryable()).Returns(userRolesMock);
            _mockRoleRepo.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(new Role { Id = 10, Name = "Admin" });

            var loginDto = new LoginDto { UserName = "sahin", Password = password };

            var token = await _authService.LoginAsync(loginDto);

            token.Should().NotBeNullOrWhiteSpace();
            token.Should().StartWith("ey");
        }

        [Fact]
        public async Task Login_WhenUserNotFound_ShouldThrowException()
        {
            var emptyUsers = new List<User>().BuildMock();
            _mockUserRepo.Setup(x => x.GetQueryable()).Returns(emptyUsers);

            var loginDto = new LoginDto { UserName = "ghost", Password = "123" };

            await _authService.Invoking(s => s.LoginAsync(loginDto))
                .Should().ThrowAsync<BusinessException>()
                .WithMessage(ErrorKeys.UserNotFound);
        }

        [Fact]
        public async Task Login_WhenPasswordIsWrong_ShouldThrowException()
        {
            var dbUser = new User { UserName = "sahin", PasswordHash = BCrypt.Net.BCrypt.HashPassword("dogru-sifre") };
            var usersMock = new List<User> { dbUser }.BuildMock();
            _mockUserRepo.Setup(x => x.GetQueryable()).Returns(usersMock);

            var loginDto = new LoginDto { UserName = "sahin", Password = "yanlis-sifre" };

            await _authService.Invoking(s => s.LoginAsync(loginDto))
                .Should().ThrowAsync<BusinessException>()
                .WithMessage(ErrorKeys.WrongPassword);
        }
    }
}