using AutoMapper;
using BusinessLayer.Abstract;
using BusinessLayer.Concrete;
using DataAccessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.Invoice;
using EntityLayer.DTOs.InvoiceDetail;
using EntityLayer.DTOs.Pagination;
using EntityLayer.DTOs.StockReceipt;
using EntityLayer.Entities.Domain;
using EntityLayer.Entities.Enums;
using EntityLayer.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FinanceApi.Tests.InvoiceTest
{
    public class InvoiceServiceTests
    {
        private readonly Mock<IInvoiceRepository> _mockInvoiceRepo;
        private readonly Mock<IInvoiceDetailRepository> _mockInvoiceDetailRepo;
        private readonly Mock<ICurrentAccountRepository> _mockCurrentAccountRepo;
        private readonly Mock<IStockReceiptService> _mockStockReceiptService;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly InvoiceService _invoiceService;

        public InvoiceServiceTests()
        {
            _mockInvoiceRepo = new Mock<IInvoiceRepository>();
            _mockInvoiceDetailRepo = new Mock<IInvoiceDetailRepository>();
            _mockCurrentAccountRepo = new Mock<ICurrentAccountRepository>();
            _mockStockReceiptService = new Mock<IStockReceiptService>();
            _mockCacheService = new Mock<ICacheService>();
            _mockMapper = new Mock<IMapper>();

            _mockInvoiceRepo.Setup(x => x.BeginTransactionAsync())
                .ReturnsAsync(new Mock<IDbContextTransaction>().Object);

            _invoiceService = new InvoiceService(
                _mockInvoiceRepo.Object,
                _mockInvoiceDetailRepo.Object,
                _mockCurrentAccountRepo.Object,
                _mockStockReceiptService.Object,
                _mockCacheService.Object,
                _mockMapper.Object);
        }

        [Fact]
        public async Task GetAllInvoicesAsync_Succesfull()
        {
            var companyId = 1;
            var filter = new InvoiceFilterDto { SerialNumber = "INV", PageNumber = 1, PageSize = 10 };
            var invoices = new List<Invoice>
            {
                new Invoice { Id = 1, CompanyId = companyId, SerialNumber = "INV001"},
                new Invoice { Id = 2, CompanyId = companyId, SerialNumber = "INV002"}
            };

            var mockQuery = invoices.BuildMock();
            _mockInvoiceRepo.Setup(x => x.GetQueryable()).Returns(mockQuery);

            _mockMapper.Setup(m => m.Map<IEnumerable<InvoiceListDto>>(It.IsAny<IEnumerable<Invoice>>()))
                .Returns(new List<InvoiceListDto> { new InvoiceListDto { SerialNumber = "INV001" } });

            var result = await _invoiceService.GetAllInvoicesAsync(filter, companyId);

            result.Should().NotBeNull();
            result.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task GetByIdAsync_WhenInvoiceExists()
        {
            var invoices = new List<Invoice> { new Invoice { Id = 1, SerialNumber = "INV001" } }.BuildMock();
            _mockInvoiceRepo.Setup(x => x.GetQueryable()).Returns(invoices);

            var dto = new InvoiceListDto { Id = 1, SerialNumber = "INV001" };
            _mockMapper.Setup(m => m.Map<InvoiceListDto>(It.IsAny<Invoice>())).Returns(dto);

            var result = await _invoiceService.GetByIdAsync(1);

            result.Should().NotBeNull();
            result.SerialNumber.Should().Be("INV001");
        }

        [Fact]
        public async Task GetByIdAsync_WhenInvoiceDoesNotExist_ShouldThrowException()
        {
            var emptyInvoices = new List<Invoice>().BuildMock();
            _mockInvoiceRepo.Setup(x => x.GetQueryable()).Returns(emptyInvoices);

            await _invoiceService.Invoking(s => s.GetByIdAsync(1))
                .Should().ThrowAsync<BusinessException>()
                .WithMessage(ErrorKeys.InvoiceNotFound);
        }

        [Fact]
        public async Task ApproveInvoiceAsync_WhenPurchase_ShouldUpdateBalanceAndCreateStockReceipt()
        {
            var invoice = new Invoice { Id = 1, Status = InvoiceStatus.Draft, Type = InvoiceType.Purchase, CurrentAccountId = 1, CompanyId = 1, WarehouseId = 1 };
            _mockInvoiceRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(invoice);

            var details = new List<InvoiceDetail> { new InvoiceDetail { InvoiceId = 1, StockId = 1, Quantity = 10, UnitPrice = 100 } }.BuildMock();
            _mockInvoiceDetailRepo.Setup(x => x.GetQueryable()).Returns(details);

            var currentAccount = new CurrentAccount { Id = 1, Balance = 2500 };
            _mockCurrentAccountRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(currentAccount);

            _mockStockReceiptService.Setup(x => x.AddAsync(It.IsAny<CreateStockReceiptDto>()))
                .ReturnsAsync(new StockReceiptListDto { Id = 99 });

            await _invoiceService.ApproveInvoiceAsync(1);

            currentAccount.Balance.Should().Be(1500);
            invoice.Status.Should().Be(InvoiceStatus.Approved);

            _mockStockReceiptService.Verify(x => x.AddAsync(It.IsAny<CreateStockReceiptDto>()), Times.Once);
            _mockInvoiceRepo.Verify(x => x.Update(invoice), Times.Once);
            _mockCurrentAccountRepo.Verify(x => x.Update(currentAccount), Times.Once);
        }

        [Fact]
        public async Task SendInvoiceToIntegratorAsync_WhenApproved_ShouldSetStatusToSent()
        {
            var invoice = new Invoice { Id = 1, Status = InvoiceStatus.Approved };
            _mockInvoiceRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(invoice);

            await _invoiceService.SendInvoiceToIntegratorAsync(1);

            invoice.Status.Should().Be(InvoiceStatus.Sent);
            _mockInvoiceRepo.Verify(x => x.Update(invoice), Times.Once);
        }
    }
}