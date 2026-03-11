using AutoMapper;
using BusinessLayer.Concrete;
using DataAccessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.Pagination;
using EntityLayer.DTOs.User;
using EntityLayer.Entities.Auth;
using EntityLayer.Exceptions;
using FluentAssertions;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace FinanceApi.Tests.UserTest
{
    public class UserServiceTests
    {
        private readonly Mock<IGenericRepository<User>> _mockUserRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockUserRepo = new Mock<IGenericRepository<User>>();
            _mockMapper = new Mock<IMapper>();
            _userService = new UserService(_mockUserRepo.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetAllUserAsync()
        {
            var filter = new UserFilterDto { Name = "sahin", PageNumber = 1, PageSize = 10 };
            var users = new List<User>
            {
                new User { Id = 1, UserName = "sahin123", FullName = "Sahin Test", CreateDate = DateTime.Now },
                new User { Id = 2, UserName = "ahmet", FullName = "Ahmet Sahin", CreateDate = DateTime.Now },
                new User { Id = 3, UserName = "mehmet", FullName = "Mehmet Kaya", CreateDate = DateTime.Now }
            };

            var mockQuery = users.BuildMock();
            _mockUserRepo.Setup(x => x.GetQueryable()).Returns(mockQuery);

            var mappedDtos = new List<UserListDto>
            {
                new UserListDto { Username = "sahin123" },
                new UserListDto { Username = "ahmet" }
            };
            _mockMapper.Setup(m => m.Map<IEnumerable<UserListDto>>(It.IsAny<IEnumerable<User>>())).Returns(mappedDtos);

            var result = await _userService.GetAllUserAsync(filter);

            result.Should().NotBeNull();
            result.TotalRecords.Should().Be(2);
            result.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdAsync_WhenUserExists()
        {
            var user = new User { Id = 1, UserName = "sahin" };
            _mockUserRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);

            var result = await _userService.GetByIdAsync(1);

            result.Should().NotBeNull();
            result.UserName.Should().Be("sahin");
        }

        [Fact]
        public async Task GetByIdAsync_WhenUserDoesNotExist()
        {
            _mockUserRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((User)null);

            Func<Task> action = async () => await _userService.GetByIdAsync(1);

            await action.Should().ThrowAsync<BusinessException>()
                .Where(x => x.Message == ErrorKeys.UserNotFound);
        }

        [Fact]
        public async Task DeleteUserAsync_WhenUserExists()
        {
            var user = new User { Id = 1 };
            _mockUserRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);

            await _userService.DeleteUserAsync(1);

            _mockUserRepo.Verify(x => x.Delete(user), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_WhenUserDoesNotExist()
        {
            _mockUserRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((User)null);

            Func<Task> action = async () => await _userService.DeleteUserAsync(1);

            await action.Should().ThrowAsync<BusinessException>()
                .Where(x => x.Message == ErrorKeys.UserNotFound);
        }

        [Fact]
        public async Task UpdateAsync_WhenUserDoesNotExist()
        {
            var dto = new UserUpdateDto { Id = 1 };
            _mockUserRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((User)null);

            Func<Task> action = async () => await _userService.UpdateAsync(dto);

            await action.Should().ThrowAsync<BusinessException>()
                .Where(x => x.Message == ErrorKeys.UserNotFound);
        }

        [Fact]
        public async Task UpdateAsync_WhenUserNameChangedAndExists()
        {
            var dto = new UserUpdateDto { Id = 1, UserName = "new_username" };
            var existingUser = new User { Id = 1, UserName = "old_username" };

            _mockUserRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingUser);
            _mockUserRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(true);

            Func<Task> action = async () => await _userService.UpdateAsync(dto);

            await action.Should().ThrowAsync<BusinessException>()
                .Where(x => x.Message == ErrorKeys.UserNameAlreadyExists);
        }

        [Fact]
        public async Task UpdateAsync_WhenValid()
        {
            var dto = new UserUpdateDto { Id = 1, UserName = "new_username", FullName = "New Name", Password = "new_password" };
            var existingUser = new User { Id = 1, UserName = "old_username", FullName = "Old Name" };

            _mockUserRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingUser);
            _mockUserRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(false);

            await _userService.UpdateAsync(dto);

            existingUser.UserName.Should().Be("new_username");
            existingUser.FullName.Should().Be("New Name");
            existingUser.PasswordHash.Should().NotBeNullOrWhiteSpace();

            _mockUserRepo.Verify(x => x.Update(existingUser), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenPasswordIsBlank()
        {
            var dto = new UserUpdateDto { Id = 1, UserName = "same_username", Password = "" };
            var existingUser = new User { Id = 1, UserName = "same_username", PasswordHash = "old_hash" };

            _mockUserRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingUser);

            await _userService.UpdateAsync(dto);

            existingUser.PasswordHash.Should().Be("old_hash");
            _mockUserRepo.Verify(x => x.Update(existingUser), Times.Once);
        }
    }
}