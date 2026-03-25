using AutoMapper;
using BusinessLayer.Concrete;
using DataAccessLayer.Abstract;
using EntityLayer.DTOs.StockWarehouse;
using EntityLayer.Entities;
using EntityLayer.Entities.Domain;
using FluentAssertions;
using MockQueryable;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FinanceApi.Tests.StockWarehouseTest
{
    public class StockWarehouseServiceTests
    {
        private readonly Mock<IGenericRepository<StockWarehouse>> _swRepoMock;
        private readonly Mock<IGenericRepository<Stock>> _stockRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly StockWarehouseService _service;

        public StockWarehouseServiceTests()
        {
            _swRepoMock = new Mock<IGenericRepository<StockWarehouse>>();
            _stockRepoMock = new Mock<IGenericRepository<Stock>>();
            _mapperMock = new Mock<IMapper>();

            _service = new StockWarehouseService(
                _swRepoMock.Object,
                _stockRepoMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task GetAll_Success()
        {
            var stock = new Stock { Id = 1, CompanyId = 1 };
            var records = new List<StockWarehouse>
            {
                new StockWarehouse { StockId = 1, WarehouseId = 1, Quantity = 10, Stock = stock },
                new StockWarehouse { StockId = 1, WarehouseId = 2, Quantity = 20, Stock = stock }
            };

            var mockQuery = records.BuildMock();
            _swRepoMock.Setup(x => x.GetQueryable()).Returns(mockQuery);

            var dtoList = new List<StockWarehouseListDto>
            {
                new StockWarehouseListDto { StockId = 1, WarehouseId = 1 },
                new StockWarehouseListDto { StockId = 1, WarehouseId = 2 }
            };

            _mapperMock.Setup(m => m.Map<List<StockWarehouseListDto>>(It.IsAny<List<StockWarehouse>>())).Returns(dtoList);

            var filter = new StockWarehouseFilterDto { CompanyId = 1 };
            var result = await _service.GetAllStockStatusAsync(filter);

            result.Should().NotBeNull();
            result.Count.Should().Be(2);
        }

        [Fact]
        public async Task GetAll_WithFilters_Success()
        {
            var stock1 = new Stock { Id = 1, CompanyId = 1 };
            var stock2 = new Stock { Id = 2, CompanyId = 1 };

            var records = new List<StockWarehouse>
            {
                new StockWarehouse { StockId = 1, WarehouseId = 1, Quantity = 10, Stock = stock1 },
                new StockWarehouse { StockId = 2, WarehouseId = 2, Quantity = 20, Stock = stock2 }
            };

            var mockQuery = records.BuildMock();
            _swRepoMock.Setup(x => x.GetQueryable()).Returns(mockQuery);

            var dtoList = new List<StockWarehouseListDto>
            {
                new StockWarehouseListDto { StockId = 1, WarehouseId = 1 }
            };

            _mapperMock.Setup(m => m.Map<List<StockWarehouseListDto>>(It.IsAny<List<StockWarehouse>>())).Returns(dtoList);

            var filter = new StockWarehouseFilterDto { CompanyId = 1, WarehouseId = 1, StockId = 1 };
            var result = await _service.GetAllStockStatusAsync(filter);

            result.Should().NotBeNull();
            result.Count.Should().Be(1);
        }
    }
}