using AutoMapper;
using BusinessLayer.Concrete;
using DataAccessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.Pagination;
using EntityLayer.DTOs.StockTrans;
using EntityLayer.Entities;
using EntityLayer.Entities.Domain;
using EntityLayer.Entities.Enums;
using EntityLayer.Exceptions;
using FluentAssertions;
using MockQueryable;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FinanceApi.Tests.StockTransTest
{
    public class StockTransServiceTests
    {
        private readonly Mock<IGenericRepository<StockTrans>> _transRepoMock;
        private readonly Mock<IGenericRepository<Stock>> _stockRepoMock;
        private readonly Mock<IGenericRepository<Warehouse>> _warehouseRepoMock;
        private readonly Mock<IGenericRepository<StockWarehouse>> _swRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly StockTransService _service;

        public StockTransServiceTests()
        {
            _transRepoMock = new Mock<IGenericRepository<StockTrans>>();
            _stockRepoMock = new Mock<IGenericRepository<Stock>>();
            _warehouseRepoMock = new Mock<IGenericRepository<Warehouse>>();
            _swRepoMock = new Mock<IGenericRepository<StockWarehouse>>();
            _mapperMock = new Mock<IMapper>();

            _service = new StockTransService(
                _transRepoMock.Object,
                _stockRepoMock.Object,
                _warehouseRepoMock.Object,
                _swRepoMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task Process_StockNotFound_Throws()
        {
            _stockRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Stock)null);

            await _service.Invoking(s => s.ProcessStockActionAsync(1, 1, 10, 100, TransactionType.In))
                .Should().ThrowAsync<BusinessException>()
                .WithMessage(ErrorKeys.StockNotFound);
        }

        [Fact]
        public async Task Process_InsufficientStock_Throws()
        {
            var stock = new Stock { Id = 1, CompanyId = 1 };
            _stockRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(stock);

            var sw = new StockWarehouse { StockId = 1, WarehouseId = 1, Quantity = 5 };
            var mockQuery = new List<StockWarehouse> { sw }.BuildMock();
            _swRepoMock.Setup(x => x.GetQueryable()).Returns(mockQuery);

            await _service.Invoking(s => s.ProcessStockActionAsync(1, 1, 10, 100, TransactionType.Out, 1))
                .Should().ThrowAsync<BusinessException>()
                .WithMessage(ErrorKeys.InsufficientStock);
        }

        [Fact]
        public async Task Process_Success()
        {
            var stock = new Stock { Id = 1, CompanyId = 1 };
            _stockRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(stock);

            await _service.ProcessStockActionAsync(1, 1, 10, 100, TransactionType.In, 1);

            _transRepoMock.Verify(x => x.AddAsync(It.IsAny<StockTrans>()), Times.Once);
        }

        [Fact]
        public async Task GetAll_Success()
        {
            var records = new List<StockTrans>
            {
                new StockTrans { Id = 1, CompanyId = 1, StockId = 1, WarehouseId = 1, CreateDate = DateTime.Now },
                new StockTrans { Id = 2, CompanyId = 1, StockId = 2, WarehouseId = 1, CreateDate = DateTime.Now }
            };

            var mockQuery = records.BuildMock();
            _transRepoMock.Setup(x => x.GetQueryable()).Returns(mockQuery);

            var dtoList = new List<StockTransListDto>
            {
                new StockTransListDto { Id = 1 },
                new StockTransListDto { Id = 2 }
            };

            _mapperMock.Setup(m => m.Map<IEnumerable<StockTransListDto>>(It.IsAny<List<StockTrans>>())).Returns(dtoList);

            var filter = new StockTransFilterDto { CompanyId = 1, PageNumber = 1, PageSize = 10 };
            var result = await _service.GetAllTransactionsAsync(filter);

            result.Should().NotBeNull();
            result.TotalRecords.Should().Be(2);
            result.Data.Should().HaveCount(2);
        }
    }
}