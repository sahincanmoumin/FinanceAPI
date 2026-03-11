using AutoMapper;
using BusinessLayer.Concrete;
using DataAccessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.Pagination;
using EntityLayer.DTOs.Role;
using EntityLayer.Entities.Domain;
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

namespace FinanceApi.Tests.RoleTest
{
    public class RoleServiceTests
    {
        private readonly Mock<IGenericRepository<Role>> _mockRoleRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly RoleService _roleService;

        public RoleServiceTests()
        {
            _mockRoleRepo = new Mock<IGenericRepository<Role>>();
            _mockMapper = new Mock<IMapper>();
            _roleService = new RoleService(_mockRoleRepo.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetAllRolesAsync()
        {
            var filter = new RoleFilterDto { Name = "Admin", PageNumber = 1, PageSize = 10 };
            var roles = new List<Role>
            {
                new Role { Id = 1, Name = "Admin", CreateDate = DateTime.Now },
                new Role { Id = 2, Name = "SuperAdmin", CreateDate = DateTime.Now },
                new Role { Id = 3, Name = "User", CreateDate = DateTime.Now }
            };

            var mockQuery = roles.BuildMock();
            _mockRoleRepo.Setup(x => x.GetQueryable()).Returns(mockQuery);

            var mappedDtos = new List<RoleListDto>
            {
                new RoleListDto { Name = "Admin" },
                new RoleListDto { Name = "SuperAdmin" }
            };
            _mockMapper.Setup(m => m.Map<IEnumerable<RoleListDto>>(It.IsAny<IEnumerable<Role>>())).Returns(mappedDtos);

            var result = await _roleService.GetAllRolesAsync(filter);

            result.Should().NotBeNull();
            result.TotalRecords.Should().Be(2);
            result.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdAsync_WhenRoleExists()
        {
            var role = new Role { Id = 1, Name = "Admin" };
            var dto = new RoleListDto { Id = 1, Name = "Admin" };

            _mockRoleRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(role);
            _mockMapper.Setup(m => m.Map<RoleListDto>(role)).Returns(dto);

            var result = await _roleService.GetByIdAsync(1);

            result.Should().NotBeNull();
            result.Name.Should().Be("Admin");
        }

        [Fact]
        public async Task GetByIdAsync_WhenRoleDoesNotExist()
        {
            _mockRoleRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Role)null);

            Func<Task> action = async () => await _roleService.GetByIdAsync(1);

            await action.Should().ThrowAsync<BusinessException>()
                .Where(x => x.Message == ErrorKeys.RoleNotFound);
        }

        [Fact]
        public async Task AddAsync_WhenRoleNameExists()
        {
            var dto = new CreateRoleDto { Name = "Admin" };
            _mockRoleRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Role, bool>>>())).ReturnsAsync(true);

            Func<Task> action = async () => await _roleService.AddAsync(dto);

            await action.Should().ThrowAsync<BusinessException>()
                .Where(x => x.Message == ErrorKeys.RoleAlreadyExists);
        }

        [Fact]
        public async Task AddAsync_WhenSuccessful()
        {
            var dto = new CreateRoleDto { Name = "NewRole" };
            var role = new Role { Name = "NewRole" };

            _mockRoleRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Role, bool>>>())).ReturnsAsync(false);
            _mockMapper.Setup(m => m.Map<Role>(dto)).Returns(role);

            await _roleService.AddAsync(dto);

            _mockRoleRepo.Verify(x => x.AddAsync(It.IsAny<Role>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenRoleExists()
        {
            var dto = new UpdateRoleDto { Id = 1, Name = "UpdatedRole" };
            var existingRole = new Role { Id = 1, Name = "OldRole" };

            _mockRoleRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingRole);

            await _roleService.UpdateAsync(dto);

            _mockMapper.Verify(m => m.Map(dto, existingRole), Times.Once);
            _mockRoleRepo.Verify(x => x.Update(existingRole), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenRoleDoesNotExist()
        {
            var dto = new UpdateRoleDto { Id = 1 };
            _mockRoleRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Role)null);

            Func<Task> action = async () => await _roleService.UpdateAsync(dto);

            await action.Should().ThrowAsync<BusinessException>()
                .Where(x => x.Message == ErrorKeys.RoleNotFound);
        }

        [Fact]
        public async Task DeleteAsync_WhenRoleExists()
        {
            var role = new Role { Id = 1 };
            _mockRoleRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(role);

            await _roleService.DeleteAsync(1);

            _mockRoleRepo.Verify(x => x.Delete(role), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenRoleDoesNotExist()
        {
            _mockRoleRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Role)null);

            Func<Task> action = async () => await _roleService.DeleteAsync(1);

            await action.Should().ThrowAsync<BusinessException>()
                .Where(x => x.Message == ErrorKeys.RoleNotFound);
        }
    }
}