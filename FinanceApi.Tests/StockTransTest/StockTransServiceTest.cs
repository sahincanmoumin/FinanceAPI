using AutoMapper;
using BusinessLayer.Concrete;
using DataAccessLayer.Abstract;
using EntityLayer.DTOs.Pagination;
using EntityLayer.DTOs.StockTrans;
using EntityLayer.Entities.Domain;
using EntityLayer.Entities.Enums;
using FluentAssertions;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FinanceApi.Tests.StockTransTest
{
    public class StockTransServiceTests
    {
        private readonly Mock<IGenericRepository<StockTrans>> _mockStockTransRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly StockTransService _stockTransService;

        public StockTransServiceTests()
        {
            _mockStockTransRepo = new Mock<IGenericRepository<StockTrans>>();
            _mockMapper = new Mock<IMapper>();
            _stockTransService = new StockTransService(_mockStockTransRepo.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetTransactionsByStockIdAsync_basic()
        {
            int stockId = 1;
            var filter = new StockTransFilterDto { PageNumber = 1, PageSize = 10 };

            var transactions = new List<StockTrans>
            {
                new StockTrans { Id = 1, StockId = stockId, Date = DateTime.Now },
                new StockTrans { Id = 2, StockId = stockId, Date = DateTime.Now.AddDays(-1) },
                new StockTrans { Id = 3, StockId = 2, Date = DateTime.Now } // Different StockId
            };

            var mockQuery = transactions.BuildMock();
            _mockStockTransRepo.Setup(x => x.GetQueryable()).Returns(mockQuery);

            var mappedDtos = new List<StockTransListDto>
            {
                new StockTransListDto { Id = 1 },
                new StockTransListDto { Id = 2 }
            };

            _mockMapper.Setup(m => m.Map<IEnumerable<StockTransListDto>>(It.IsAny<IEnumerable<StockTrans>>()))
                       .Returns(mappedDtos);

            var result = await _stockTransService.GetTransactionsByStockIdAsync(stockId, filter);

            result.Should().NotBeNull();
            result.TotalRecords.Should().Be(2);
            result.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetTransactionsByStockIdAsync_ShouldApplyAllFiltersCorrectly()
        {
            int stockId = 1;
            var baseDate = new DateTime(2024, 1, 15);
            var filter = new StockTransFilterDto
            {
                PageNumber = 1,
                PageSize = 10,
                Direction = TransactionType.In,
                StartDate = new DateTime(2024, 1, 10),
                EndDate = new DateTime(2024, 1, 20),
                MinQuantity = 5,
                MaxQuantity = 20,
                MinUnitPrice = 100,
                MaxUnitPrice = 500
            };

            var transactions = new List<StockTrans>
            {
                new StockTrans { Id = 1, StockId = stockId, Direction = TransactionType.In, Date = baseDate, Quantity = 10, UnitPrice = 200 },
                
                new StockTrans { Id = 2, StockId = stockId, Direction = TransactionType.Out, Date = baseDate, Quantity = 10, UnitPrice = 200 },
                
                new StockTrans { Id = 3, StockId = stockId, Direction = TransactionType.In, Date = new DateTime(2024, 2, 1), Quantity = 10, UnitPrice = 200 },
                
                new StockTrans { Id = 4, StockId = stockId, Direction = TransactionType.In, Date = baseDate, Quantity = 50, UnitPrice = 200 },
                
                new StockTrans { Id = 5, StockId = stockId, Direction = TransactionType.In, Date = baseDate, Quantity = 10, UnitPrice = 50 }
            };

            var mockQuery = transactions.BuildMock();
            _mockStockTransRepo.Setup(x => x.GetQueryable()).Returns(mockQuery);

            var mappedDtos = new List<StockTransListDto> { new StockTransListDto { Id = 1 } };

            _mockMapper.Setup(m => m.Map<IEnumerable<StockTransListDto>>(It.IsAny<IEnumerable<StockTrans>>()))
                       .Returns(mappedDtos);

            var result = await _stockTransService.GetTransactionsByStockIdAsync(stockId, filter);

            result.Should().NotBeNull();
            result.TotalRecords.Should().Be(1);
            result.Data.Should().HaveCount(1);
        }
    }
}