using AutoMapper;
using BusinessLayer.Concrete;
using DataAccessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.Pagination;
using EntityLayer.DTOs.Stock;
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

namespace FinanceApi.Tests.StockTest
{
    public class StockServiceTests
    {
        private readonly Mock<IStockRepository> _mockStockRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly StockService _stockService;

        public StockServiceTests()
        {
            _mockStockRepo = new Mock<IStockRepository>();
            _mockMapper = new Mock<IMapper>();
            _stockService = new StockService(_mockStockRepo.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetAllStocksAsync_ShouldReturnFilteredPagedData()
        {
            var companyId = 1;
            var filter = new StockFilterDto { Name = "Laptop", Code = "LPT", PageNumber = 1, PageSize = 10 };
            var stocks = new List<Stock>
            {
                new Stock { Id = 1, CompanyId = companyId, Name = "Laptop Pro", Code = "LPT-01", Balance = 50, CreateDate = DateTime.Now },
                new Stock { Id = 2, CompanyId = companyId, Name = "Laptop Air", Code = "LPT-02", Balance = 30, CreateDate = DateTime.Now },
                new Stock { Id = 3, CompanyId = 2, Name = "Desktop", Code = "DSK-01", Balance = 10, CreateDate = DateTime.Now }
            };

            var mockQuery = stocks.BuildMock();
            _mockStockRepo.Setup(x => x.GetQueryable()).Returns(mockQuery);

            var mappedDtos = new List<StockListDto>
            {
                new StockListDto { Name = "Laptop Pro", Code = "LPT-01" },
                new StockListDto { Name = "Laptop Air", Code = "LPT-02" }
            };
            _mockMapper.Setup(m => m.Map<IEnumerable<StockListDto>>(It.IsAny<IEnumerable<Stock>>())).Returns(mappedDtos);

            var result = await _stockService.GetAllStocksAsync(filter, companyId);

            result.Should().NotBeNull();
            result.TotalRecords.Should().Be(2);
            result.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdAsync_WhenStockExists_ShouldReturnMappedDto()
        {
            var stock = new Stock { Id = 1, Name = "Monitor" };
            var dto = new StockListDto { Id = 1, Name = "Monitor" };

            _mockStockRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(stock);
            _mockMapper.Setup(m => m.Map<StockListDto>(stock)).Returns(dto);

            var result = await _stockService.GetByIdAsync(1);

            result.Should().NotBeNull();
            result.Name.Should().Be("Monitor");
        }

        [Fact]
        public async Task GetByIdAsync_WhenStockDoesNotExist()
        {
            _mockStockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Stock)null);

            Func<Task> action = async () => await _stockService.GetByIdAsync(1);

            await action.Should().ThrowAsync<BusinessException>()
                .Where(x => x.Message == ErrorKeys.StockNotFound);
        }

        [Fact]
        public async Task AddAsync_WhenStockCodeExists()
        {
            var dto = new CreateStockDto { Code = "STK-001", CompanyId = 1 };
            _mockStockRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Stock, bool>>>())).ReturnsAsync(true);

            Func<Task> action = async () => await _stockService.AddAsync(dto);

            await action.Should().ThrowAsync<BusinessException>()
                .Where(x => x.Message == ErrorKeys.StockAlreadyExists);
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
        }

        [Fact]
        public async Task UpdateAsync_WhenStockExists()
        {
            var dto = new UpdateStockDto { Id = 1, Name = "Updated Stock" };
            var existingStock = new Stock { Id = 1, Name = "Old Stock" };

            _mockStockRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingStock);

            await _stockService.UpdateAsync(dto);

            _mockMapper.Verify(m => m.Map(dto, existingStock), Times.Once);
            _mockStockRepo.Verify(x => x.Update(existingStock), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenStockDoesNotExist()
        {
            var dto = new UpdateStockDto { Id = 1 };
            _mockStockRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Stock)null);

            Func<Task> action = async () => await _stockService.UpdateAsync(dto);

            await action.Should().ThrowAsync<BusinessException>()
                .Where(x => x.Message == ErrorKeys.StockNotFound);
        }

        [Fact]
        public async Task DeleteAsync_WhenStockExists()
        {
            var stock = new Stock { Id = 1 };
            _mockStockRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(stock);

            await _stockService.DeleteAsync(1);

            _mockStockRepo.Verify(x => x.Delete(stock), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenStockDoesNotExist()
        {
            _mockStockRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Stock)null);

            Func<Task> action = async () => await _stockService.DeleteAsync(1);

            await action.Should().ThrowAsync<BusinessException>()
                .Where(x => x.Message == ErrorKeys.StockNotFound);
        }
    }
}