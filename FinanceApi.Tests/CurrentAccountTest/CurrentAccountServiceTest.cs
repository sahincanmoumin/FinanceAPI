using AutoMapper;
using BusinessLayer.Concrete;
using DataAccessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.CurrentAccount;
using EntityLayer.Entities.Domain;
using EntityLayer.Entities.Enums;
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

namespace FinanceApi.Tests.CurrentAccountTest
{
    public class CurrentAccountServiceTests
    {
        private readonly Mock<ICurrentAccountRepository> _mockRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly CurrentAccountService _currentAccountService;

        public CurrentAccountServiceTests()
        {
            _mockRepo = new Mock<ICurrentAccountRepository>();
            _mockMapper = new Mock<IMapper>();
            _currentAccountService = new CurrentAccountService(_mockRepo.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetByIdAsync_WhenAccountExists()
        {
            var account = new CurrentAccount { Id = 1, Name = "Test Account" };
            var dto = new CurrentAccountListDto { Id = 1, Name = "Test Account" };

            _mockRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(account);
            _mockMapper.Setup(m => m.Map<CurrentAccountListDto>(account)).Returns(dto);

            var result = await _currentAccountService.GetByIdAsync(1);

            result.Should().NotBeNull();
            result.Name.Should().Be("Test Account");
        }

        [Fact]
        public async Task GetByIdAsync_WhenAccountDoesNotExist()
        {
            _mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((CurrentAccount)null);

            Func<Task> action = async () => await _currentAccountService.GetByIdAsync(1);

            await action.Should().ThrowAsync<BusinessException>()
                .Where(x => x.Message == ErrorKeys.CurrentAccountNotFound);
        }

        [Fact]
        public async Task AddAsync_WhenAccountCodeExists()
        {
            var dto = new CreateCurrentAccountDto { Code = "ACC001", CompanyId = 1 };
            _mockRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<CurrentAccount, bool>>>())).ReturnsAsync(true);

            Func<Task> action = async () => await _currentAccountService.AddAsync(dto);

            await action.Should().ThrowAsync<BusinessException>()
                .Where(x => x.Message == ErrorKeys.CurrentAccountAlreadyExists);
        }

        [Fact]
        public async Task AddAsync_WhenInvalidAccountType()
        {
            var dto = new CreateCurrentAccountDto { Code = "ACC001", CompanyId = 1, Type = (AccountType)99 };
            _mockRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<CurrentAccount, bool>>>())).ReturnsAsync(false);

            Func<Task> action = async () => await _currentAccountService.AddAsync(dto);

            await action.Should().ThrowAsync<BusinessException>()
                .Where(x => x.Message == ErrorKeys.InvalidAccountType);
        }

        [Fact]
        public async Task AddAsync_WhenSuccessful()
        {
            var dto = new CreateCurrentAccountDto { Code = "ACC001", CompanyId = 1, Type = AccountType.Buyer, Balance = 100 };
            var account = new CurrentAccount { Code = "ACC001", CompanyId = 1, Balance = 100 };

            _mockRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<CurrentAccount, bool>>>())).ReturnsAsync(false);
            _mockMapper.Setup(m => m.Map<CurrentAccount>(dto)).Returns(account);

            await _currentAccountService.AddAsync(dto);

            _mockRepo.Verify(x => x.AddAsync(It.IsAny<CurrentAccount>()), Times.Once);
        }

        [Fact]
        public async Task GetAllCurrentAccountsAsync()
        {
            int companyId = 1;
            var filter = new CurrentAccountFilterDto { Name = "Supplier", PageNumber = 1, PageSize = 10 };
            var accounts = new List<CurrentAccount>
            {
                new CurrentAccount { Id = 1, Name = "Supplier A", CompanyId = companyId, CreateDate = DateTime.Now },
                new CurrentAccount { Id = 2, Name = "Customer B", CompanyId = companyId, CreateDate = DateTime.Now },
                new CurrentAccount { Id = 3, Name = "Supplier C", CompanyId = 2, CreateDate = DateTime.Now }
            };

            var mockQuery = accounts.BuildMock();
            _mockRepo.Setup(x => x.GetQueryable()).Returns(mockQuery);

            var mappedDtos = new List<CurrentAccountListDto> { new CurrentAccountListDto { Name = "Supplier A" } };
            _mockMapper.Setup(m => m.Map<IEnumerable<CurrentAccountListDto>>(It.IsAny<IEnumerable<CurrentAccount>>())).Returns(mappedDtos);

            var result = await _currentAccountService.GetAllCurrentAccountsAsync(filter, companyId);

            result.Should().NotBeNull();
            result.TotalRecords.Should().Be(1);
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task UpdateAsync_WhenAccountExists()
        {
            var dto = new UpdateCurrentAccountDto { Id = 1, Name = "Updated Name" };
            var existingAccount = new CurrentAccount { Id = 1, Name = "Old Name" };

            _mockRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingAccount);

            await _currentAccountService.UpdateAsync(dto);

            _mockMapper.Verify(m => m.Map(dto, existingAccount), Times.Once);
            _mockRepo.Verify(x => x.Update(existingAccount), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenAccountDoesNotExist()
        {
            var dto = new UpdateCurrentAccountDto { Id = 1 };
            _mockRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((CurrentAccount)null);

            Func<Task> action = async () => await _currentAccountService.UpdateAsync(dto);

            await action.Should().ThrowAsync<BusinessException>()
                .Where(x => x.Message == ErrorKeys.CurrentAccountNotFound);
        }

        [Fact]
        public async Task DeleteAsync_WhenAccountExists()
        {
            var account = new CurrentAccount { Id = 1 };
            _mockRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(account);

            await _currentAccountService.DeleteAsync(1);

            _mockRepo.Verify(x => x.Delete(account), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenAccountDoesNotExist()
        {
            _mockRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((CurrentAccount)null);

            Func<Task> action = async () => await _currentAccountService.DeleteAsync(1);

            await action.Should().ThrowAsync<BusinessException>()
                .Where(x => x.Message == ErrorKeys.CurrentAccountNotFound);
        }
    }
}