using AutoMapper;
using BusinessLayer.Abstract;
using BusinessLayer.Concrete;
using DataAccessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.CurrentAccount;
using EntityLayer.DTOs.Pagination;
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
        private readonly Mock<ICurrentAccountRepository> _mockCurrentAccountRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ICacheService> _mockCacheService; 
        private readonly CurrentAccountService _currentAccountService;

        public CurrentAccountServiceTests()
        {
            _mockCurrentAccountRepo = new Mock<ICurrentAccountRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockCacheService = new Mock<ICacheService>(); 

            _currentAccountService = new CurrentAccountService(
                _mockCurrentAccountRepo.Object,
                _mockMapper.Object,
                _mockCacheService.Object);
        }

        [Fact]
        public async Task GetAllCurrentAccountsAsync_ShouldReturnFilteredPagedData()
        {
            var companyId = 1;
            var filter = new CurrentAccountFilterDto { Name = "Ahmet", PageNumber = 1, PageSize = 10 };
            var accounts = new List<CurrentAccount>
            {
                new CurrentAccount { Id = 1, CompanyId = companyId, Name = "Ahmet Yilmaz", Code = "C-01", CreateDate = DateTime.Now },
                new CurrentAccount { Id = 2, CompanyId = companyId, Name = "Ahmet Kaya", Code = "C-02", CreateDate = DateTime.Now }
            };

            var mockQuery = accounts.BuildMock();
            _mockCurrentAccountRepo.Setup(x => x.GetQueryable()).Returns(mockQuery);

            var mappedDtos = new List<CurrentAccountListDto>
            {
                new CurrentAccountListDto { Name = "Ahmet Yilmaz", Code = "C-01" },
                new CurrentAccountListDto { Name = "Ahmet Kaya", Code = "C-02" }
            };

            _mockMapper.Setup(m => m.Map<IEnumerable<CurrentAccountListDto>>(It.IsAny<IEnumerable<CurrentAccount>>()))
                       .Returns(mappedDtos);

            var result = await _currentAccountService.GetAllCurrentAccountsAsync(filter, companyId);

            result.Should().NotBeNull();
            result.TotalRecords.Should().Be(2);
            result.Data.Should().HaveCount(2);

            _mockCacheService.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<PagedResponse<CurrentAccountListDto>>(), 60), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WhenAccountExists_ShouldReturnMappedDto()
        {
            var account = new CurrentAccount { Id = 1, Name = "Mehmet" };
            var dto = new CurrentAccountListDto { Id = 1, Name = "Mehmet" };

            _mockCurrentAccountRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(account);
            _mockMapper.Setup(m => m.Map<CurrentAccountListDto>(account)).Returns(dto);

            var result = await _currentAccountService.GetByIdAsync(1);

            result.Should().NotBeNull();
            result.Name.Should().Be("Mehmet");

            _mockCacheService.Verify(c => c.SetAsync(It.Is<string>(s => s == "CurrentAccount_Single_1"), It.IsAny<CurrentAccountListDto>(), 60), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WhenAccountDoesNotExist_ShouldThrowBusinessException()
        {
            _mockCurrentAccountRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((CurrentAccount)null);

            Func<Task> action = async () => await _currentAccountService.GetByIdAsync(1);

            await action.Should().ThrowAsync<BusinessException>()
                .Where(x => x.Message == ErrorKeys.CurrentAccountNotFound);
        }

        [Fact]
        public async Task AddAsync_WhenCodeExists_ShouldThrowBusinessException()
        {
            var dto = new CreateCurrentAccountDto { Code = "C-001", CompanyId = 1 };
            _mockCurrentAccountRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<CurrentAccount, bool>>>())).ReturnsAsync(true);

            Func<Task> action = async () => await _currentAccountService.AddAsync(dto);

            await action.Should().ThrowAsync<BusinessException>()
                .Where(x => x.Message == ErrorKeys.CurrentAccountAlreadyExists);
        }

        [Fact]
        public async Task AddAsync_WhenSuccessful_ShouldCallRepositoryAndClearCache()
        {
            var dto = new CreateCurrentAccountDto { Code = "C-002", CompanyId = 1, Type = (AccountType)1 };
            var account = new CurrentAccount { Code = "C-002", CompanyId = 1, Balance = -10 };

            _mockCurrentAccountRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<CurrentAccount, bool>>>())).ReturnsAsync(false);
            _mockMapper.Setup(m => m.Map<CurrentAccount>(dto)).Returns(account);

            await _currentAccountService.AddAsync(dto);

            account.Balance.Should().Be(0);
            _mockCurrentAccountRepo.Verify(x => x.AddAsync(It.IsAny<CurrentAccount>()), Times.Once);

            _mockCacheService.Verify(c => c.RemoveByPatternAsync(It.Is<string>(s => s.StartsWith("CurrentAccounts_Company_1"))), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenAccountExists_ShouldUpdateAndClearCache()
        {
            var dto = new UpdateCurrentAccountDto { Id = 1, Name = "Updated Name" };
            var existingAccount = new CurrentAccount { Id = 1, Name = "Old Name", CompanyId = 1 };

            _mockCurrentAccountRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingAccount);

            await _currentAccountService.UpdateAsync(dto);

            _mockMapper.Verify(m => m.Map(dto, existingAccount), Times.Once);
            _mockCurrentAccountRepo.Verify(x => x.Update(existingAccount), Times.Once);

            _mockCacheService.Verify(c => c.RemoveAsync("CurrentAccount_Single_1"), Times.Once);
            _mockCacheService.Verify(c => c.RemoveByPatternAsync(It.Is<string>(s => s.StartsWith("CurrentAccounts_Company_1"))), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenAccountDoesNotExist_ShouldThrowBusinessException()
        {
            var dto = new UpdateCurrentAccountDto { Id = 1 };
            _mockCurrentAccountRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((CurrentAccount)null);

            Func<Task> action = async () => await _currentAccountService.UpdateAsync(dto);

            await action.Should().ThrowAsync<BusinessException>()
                .Where(x => x.Message == ErrorKeys.CurrentAccountNotFound);
        }

        [Fact]
        public async Task DeleteAsync_WhenAccountExists_ShouldDeleteAndClearCache()
        {
            var account = new CurrentAccount { Id = 1, CompanyId = 1 };
            _mockCurrentAccountRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(account);

            await _currentAccountService.DeleteAsync(1);

            _mockCurrentAccountRepo.Verify(x => x.Delete(account), Times.Once);

            _mockCacheService.Verify(c => c.RemoveAsync("CurrentAccount_Single_1"), Times.Once);
            _mockCacheService.Verify(c => c.RemoveByPatternAsync(It.Is<string>(s => s.StartsWith("CurrentAccounts_Company_1"))), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenAccountDoesNotExist_ShouldThrowBusinessException()
        {
            _mockCurrentAccountRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((CurrentAccount)null);

            Func<Task> action = async () => await _currentAccountService.DeleteAsync(1);

            await action.Should().ThrowAsync<BusinessException>()
                .Where(x => x.Message == ErrorKeys.CurrentAccountNotFound);
        }
    }
}