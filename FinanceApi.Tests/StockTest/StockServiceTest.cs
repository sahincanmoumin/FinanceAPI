using AutoMapper;
using BusinessLayer.Abstract;
using BusinessLayer.Concrete;
using DataAccessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.Pagination;
using EntityLayer.DTOs.Stock;
using EntityLayer.Entities.Domain;
using EntityLayer.Exceptions;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FinanceApi.Tests.StockTest
{
    public class StockServiceTests
    {
        private readonly Mock<IStockRepository> _mockStockRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<IValidator<CreateStockDto>> _mockCreateValidator;
        private readonly Mock<IValidator<UpdateStockDto>> _mockUpdateValidator;
        private readonly StockService _stockService;

        public StockServiceTests()
        {
            _mockStockRepo = new Mock<IStockRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockCacheService = new Mock<ICacheService>();
            _mockCreateValidator = new Mock<IValidator<CreateStockDto>>();
            _mockUpdateValidator = new Mock<IValidator<UpdateStockDto>>();

            // Validation Setup - Context yerine direkt nesne tipi üzerinden
            _mockCreateValidator
                .Setup(v => v.ValidateAsync(It.IsAny<CreateStockDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockUpdateValidator
                .Setup(v => v.ValidateAsync(It.IsAny<UpdateStockDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _stockService = new StockService(
                _mockStockRepo.Object,
                _mockMapper.Object,
                _mockCacheService.Object,
                _mockCreateValidator.Object,
                _mockUpdateValidator.Object);
        }

        [Fact]
        public async Task GetAllStocksAsync_Succesfull()
        {
            var companyId = 1;
            var filter = new StockFilterDto { Name = "Laptop", PageNumber = 1, PageSize = 10 };
            var stocks = new List<Stock>
            {
                new Stock { Id = 1, CompanyId = companyId, Name = "Laptop Pro", Code = "LPT-01" },
                new Stock { Id = 2, CompanyId = companyId, Name = "Laptop Air", Code = "LPT-02" }
            };

            var mockQuery = stocks.BuildMock();
            _mockStockRepo.Setup(x => x.GetQueryable()).Returns(mockQuery);

            var mappedDtos = new List<StockListDto> { new StockListDto { Name = "Laptop Pro" } };
            _mockMapper.Setup(m => m.Map<IEnumerable<StockListDto>>(It.IsAny<IEnumerable<Stock>>())).Returns(mappedDtos);

            var result = await _stockService.GetAllStocksAsync(filter, companyId);

            result.Should().NotBeNull();
            _mockCacheService.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<PagedResponse<StockListDto>>(), It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WhenStockExists()
        {
            var stock = new Stock { Id = 1, Name = "Monitor" };
            var dto = new StockListDto { Id = 1, Name = "Monitor" };

            _mockStockRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(stock);
            _mockMapper.Setup(m => m.Map<StockListDto>(stock)).Returns(dto);

            var result = await _stockService.GetByIdAsync(1);

            result.Should().NotBeNull();
            _mockCacheService.Verify(c => c.SetAsync(It.Is<string>(s => s.Contains("1")), It.IsAny<StockListDto>(), It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WhenStockDoesNotExist_ShouldThrowException()
        {
            _mockStockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Stock)null);

            await _stockService.Invoking(s => s.GetByIdAsync(1))
                .Should().ThrowAsync<BusinessException>()
                .WithMessage(ErrorKeys.StockNotFound);
        }

        [Fact]
        public async Task AddAsync_WhenStockCodeExists_ShouldThrowException()
        {
            var dto = new CreateStockDto { Code = "STK-001", CompanyId = 1 };
            _mockStockRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Stock, bool>>>())).ReturnsAsync(true);

            await _stockService.Invoking(s => s.AddAsync(dto))
                .Should().ThrowAsync<BusinessException>()
                .WithMessage(ErrorKeys.StockAlreadyExists);
        }

        [Fact]
        public async Task AddAsync_WhenSuccessful()
        {
            var dto = new CreateStockDto { Code = "STK-002", CompanyId = 1 };
            var stock = new Stock { Code = "STK-002", CompanyId = 1 };

            _mockStockRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Stock, bool>>>())).ReturnsAsync(false);
            _mockMapper.Setup(m => m.Map<Stock>(dto)).Returns(stock);

            await _stockService.AddAsync(dto);

            _mockStockRepo.Verify(x => x.AddAsync(It.IsAny<Stock>()), Times.Once);
            _mockCacheService.Verify(c => c.RemoveByPatternAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenStockExists_ShouldUpdate()
        {
            var dto = new UpdateStockDto { Id = 1, Name = "Updated Stock" };
            var existingStock = new Stock { Id = 1, Name = "Old Stock", CompanyId = 1 };

            _mockStockRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingStock);

            await _stockService.UpdateAsync(dto);

            _mockMapper.Verify(m => m.Map(dto, existingStock), Times.Once);
            _mockStockRepo.Verify(x => x.Update(existingStock), Times.Once);
            _mockCacheService.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task DeleteAsync_WhenStockExists_ShouldDelete()
        {
            var stock = new Stock { Id = 1, CompanyId = 1 };
            _mockStockRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(stock);

            await _stockService.DeleteAsync(1);

            _mockStockRepo.Verify(x => x.Delete(stock), Times.Once);
            _mockCacheService.Verify(c => c.RemoveByPatternAsync(It.IsAny<string>()), Times.Once);
        }
    }
}