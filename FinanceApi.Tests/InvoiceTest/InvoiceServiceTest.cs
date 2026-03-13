using AutoMapper;
using BusinessLayer.Abstract;
using BusinessLayer.Concrete;
using DataAccessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.Invoice;
using EntityLayer.DTOs.InvoiceDetail;
using EntityLayer.DTOs.Pagination;
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
        private readonly Mock<IStockRepository> _mockStockRepo;
        private readonly Mock<ICurrentAccountRepository> _mockCurrentAccountRepo;
        private readonly Mock<IStockTransRepository> _mockStockTransRepo;
        private readonly Mock<IStockTransService> _mockStockTransService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly InvoiceService _invoiceService;

        public InvoiceServiceTests()
        {
            _mockInvoiceRepo = new Mock<IInvoiceRepository>();
            _mockInvoiceDetailRepo = new Mock<IInvoiceDetailRepository>();
            _mockStockRepo = new Mock<IStockRepository>();
            _mockCurrentAccountRepo = new Mock<ICurrentAccountRepository>();
            _mockStockTransRepo = new Mock<IStockTransRepository>();
            _mockStockTransService = new Mock<IStockTransService>();
            _mockMapper = new Mock<IMapper>();

            _mockInvoiceRepo.Setup(x => x.BeginTransactionAsync())
                .ReturnsAsync(new Mock<IDbContextTransaction>().Object);

            _invoiceService = new InvoiceService(
                _mockInvoiceRepo.Object,
                _mockInvoiceDetailRepo.Object,
                _mockStockRepo.Object,
                _mockCurrentAccountRepo.Object,
                _mockStockTransRepo.Object,
                _mockMapper.Object,
                _mockStockTransService.Object);
        }

        [Fact]
        public async Task GetAllInvoicesAsync_Succesfull()
        {
            var companyId = 1;
            var filter = new InvoiceFilterDto { SerialNumber = "INV", PageNumber = 1, PageSize = 10 };
            var invoices = new List<Invoice>
            {
                new Invoice { Id = 1, CompanyId = companyId, SerialNumber = "INV001", Date = DateTime.Now },
                new Invoice { Id = 2, CompanyId = companyId, SerialNumber = "INV002", Date = DateTime.Now }
            };

            var mockQuery = invoices.BuildMock();
            _mockInvoiceRepo.Setup(x => x.GetQueryable()).Returns(mockQuery);

            var mappedDtos = new List<InvoiceListDto> { new InvoiceListDto { SerialNumber = "INV001" } };
            _mockMapper.Setup(m => m.Map<IEnumerable<InvoiceListDto>>(It.IsAny<IEnumerable<Invoice>>())).Returns(mappedDtos);

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
        public async Task CreateDraftInvoiceAsync_WhenValid_ShouldSaveAsDraft()
        {
            var dto = new CreateInvoiceDto
            {
                CompanyId = 1,
                CurrentAccountId = 1,
                SerialNumber = "INV001",
                Type = InvoiceType.Purchase,
                InvoiceDetails = new List<CreateInvoiceDetailDto>
                {
                    new CreateInvoiceDetailDto { StockId = 1, Quantity = 10, UnitPrice = 50 }
                }
            };

            await _invoiceService.CreateDraftInvoiceAsync(dto);

            _mockInvoiceRepo.Verify(x => x.AddAsync(It.Is<Invoice>(i => i.Status == InvoiceStatus.Draft)), Times.Once);
            _mockInvoiceDetailRepo.Verify(x => x.AddAsync(It.IsAny<InvoiceDetail>()), Times.Once);
        }

        [Fact]
        public async Task ApproveInvoiceAsync_WhenInvoiceNotDraft_ShouldThrowException()
        {
            var invoice = new Invoice { Id = 1, Status = InvoiceStatus.Approved };
            _mockInvoiceRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(invoice);

            Func<Task> action = async () => await _invoiceService.ApproveInvoiceAsync(1);

            await action.Should().ThrowAsync<BusinessException>()
                .WithMessage(ErrorKeys.InvoiceNotDraft);
        }

        [Fact]
        public async Task ApproveInvoiceAsync_WhenPurchase_ShouldUpdateBalanceAndStocks()
        {
            var invoice = new Invoice { Id = 1, Status = InvoiceStatus.Draft, Type = InvoiceType.Purchase, CurrentAccountId = 1, CompanyId = 1 };
            _mockInvoiceRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(invoice);

            var details = new List<InvoiceDetail> { new InvoiceDetail { InvoiceId = 1, StockId = 1, Quantity = 10, UnitPrice = 100 } };
            _mockInvoiceDetailRepo.Setup(x => x.GetQueryable()).Returns(details.BuildMock());

            var currentAccount = new CurrentAccount { Id = 1, Balance = 500 };
            _mockCurrentAccountRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(currentAccount);

            await _invoiceService.ApproveInvoiceAsync(1);

            currentAccount.Balance.Should().Be(1500);
            invoice.Status.Should().Be(InvoiceStatus.Approved);

            _mockStockTransService.Verify(x => x.ProcessStockActionAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<TransactionType>()), Times.Once);

            _mockInvoiceRepo.Verify(x => x.Update(invoice), Times.Once);
        }

        [Fact]
        public async Task SendInvoiceToIntegratorAsync_WhenNotApproved_ShouldThrowException()
        {
            var invoice = new Invoice { Id = 1, Status = InvoiceStatus.Draft };
            _mockInvoiceRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(invoice);

            await _invoiceService.Invoking(s => s.SendInvoiceToIntegratorAsync(1))
                .Should().ThrowAsync<BusinessException>()
                .WithMessage(ErrorKeys.InvalidTransaction);
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