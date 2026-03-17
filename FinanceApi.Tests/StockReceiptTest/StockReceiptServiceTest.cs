using AutoMapper;
using BusinessLayer.Abstract;
using BusinessLayer.Concrete;
using DataAccessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.Entities.Domain;
using EntityLayer.Entities.Enums;
using EntityLayer.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FinanceApi.Tests.StockReceiptTest
{
    public class StockReceiptServiceTests
    {
        private readonly Mock<IGenericRepository<StockReceipt>> _mockReceiptRepo;
        private readonly Mock<IStockTransService> _mockTransService;
        private readonly Mock<IStockWarehouseService> _mockSwService;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly StockReceiptService _stockReceiptService;

        public StockReceiptServiceTests()
        {
            _mockReceiptRepo = new Mock<IGenericRepository<StockReceipt>>();
            _mockTransService = new Mock<IStockTransService>();
            _mockSwService = new Mock<IStockWarehouseService>();
            _mockCacheService = new Mock<ICacheService>();
            _mockMapper = new Mock<IMapper>();

            _mockReceiptRepo.Setup(x => x.BeginTransactionAsync())
                .ReturnsAsync(new Mock<IDbContextTransaction>().Object);

            _stockReceiptService = new StockReceiptService(
                _mockReceiptRepo.Object,
                _mockTransService.Object,
                _mockSwService.Object,
                _mockCacheService.Object,
                _mockMapper.Object
            );
        }

        [Fact]
        public async Task ApproveAsync_WhenReceiptIsNotDraft_ShouldThrowException()
        {
            var receipt = new StockReceipt { Id = 1, Status = ReceiptStatus.Approved };
            var mockQuery = new List<StockReceipt> { receipt }.BuildMock();
            _mockReceiptRepo.Setup(x => x.GetQueryable()).Returns(mockQuery);

            await _stockReceiptService.Invoking(s => s.ApproveAsync(1))
                .Should().ThrowAsync<BusinessException>()
                .WithMessage(ErrorKeys.OnlyDraftReceiptsCanBeApproved);
        }

        [Fact]
        public async Task ApproveAsync_WhenSuccess_ShouldProcessStockActions()
        {
            var receipt = new StockReceipt
            {
                Id = 1,
                Status = ReceiptStatus.Draft,
                Type = ReceiptType.Input,
                CompanyId = 1,
                WarehouseId = 10,
                Details = new List<StockReceiptDetail>
                {
                    new StockReceiptDetail { StockId = 5, Quantity = 100, UnitPrice = 10 }
                }
            };

            var mockQuery = new List<StockReceipt> { receipt }.BuildMock();
            _mockReceiptRepo.Setup(x => x.GetQueryable()).Returns(mockQuery);

            await _stockReceiptService.ApproveAsync(1);

            _mockTransService.Verify(x => x.ProcessStockActionAsync(
                receipt.CompanyId, 5, 100, 10, TransactionType.In, 10, 1), Times.Once);

            _mockSwService.Verify(x => x.UpdateBalanceAsync(5, 10, 100, ReceiptType.Input), Times.Once);
            _mockCacheService.Verify(x => x.RemoveByPatternAsync(It.IsAny<string>()), Times.Once);
            receipt.Status.Should().Be(ReceiptStatus.Approved);
        }
    }
}