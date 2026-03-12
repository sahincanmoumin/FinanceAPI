using AutoMapper;
using BusinessLayer.Concrete;
using DataAccessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.Invoice;
using EntityLayer.DTOs.InvoiceDetail;
using EntityLayer.Entities.Domain;
using EntityLayer.Entities.Enums;
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
using System.Threading;
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
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IValidator<CreateInvoiceDto>> _mockCreateValidator;
        private readonly InvoiceService _invoiceService;

        public InvoiceServiceTests()
        {
            _mockInvoiceRepo = new Mock<IInvoiceRepository>();
            _mockInvoiceDetailRepo = new Mock<IInvoiceDetailRepository>();
            _mockStockRepo = new Mock<IStockRepository>();
            _mockCurrentAccountRepo = new Mock<ICurrentAccountRepository>();
            _mockStockTransRepo = new Mock<IStockTransRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockCreateValidator = new Mock<IValidator<CreateInvoiceDto>>();

            _mockCreateValidator
                .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<CreateInvoiceDto>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _invoiceService = new InvoiceService(
                _mockInvoiceRepo.Object,
                _mockInvoiceDetailRepo.Object,
                _mockStockRepo.Object,
                _mockCurrentAccountRepo.Object,
                _mockStockTransRepo.Object,
                _mockMapper.Object,
                _mockCreateValidator.Object);
        }

        [Fact]
        public async Task GetAllInvoicesAsync()
        {
            var companyId = 1;
            var filter = new InvoiceFilterDto { SerialNumber = "INV", PageNumber = 1, PageSize = 10 };
            var invoices = new List<Invoice>
            {
                new Invoice { Id = 1, CompanyId = companyId, SerialNumber = "INV001", Date = DateTime.Now, InvoiceDetails = new List<InvoiceDetail>() },
                new Invoice { Id = 2, CompanyId = companyId, SerialNumber = "INV002", Date = DateTime.Now, InvoiceDetails = new List<InvoiceDetail>() },
                new Invoice { Id = 3, CompanyId = 2, SerialNumber = "INV003", Date = DateTime.Now, InvoiceDetails = new List<InvoiceDetail>() }
            };

            var mockQuery = invoices.BuildMock();
            _mockInvoiceRepo.Setup(x => x.GetQueryable()).Returns(mockQuery);

            var mappedDtos = new List<InvoiceListDto> { new InvoiceListDto { SerialNumber = "INV001" }, new InvoiceListDto { SerialNumber = "INV002" } };
            _mockMapper.Setup(m => m.Map<IEnumerable<InvoiceListDto>>(It.IsAny<IEnumerable<Invoice>>())).Returns(mappedDtos);

            var result = await _invoiceService.GetAllInvoicesAsync(filter, companyId);

            result.Should().NotBeNull();
            result.TotalRecords.Should().Be(2);
            result.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdAsync_WhenInvoiceExists()
        {
            var invoices = new List<Invoice> { new Invoice { Id = 1, SerialNumber = "INV001" } };
            var mockQuery = invoices.BuildMock();
            _mockInvoiceRepo.Setup(x => x.GetQueryable()).Returns(mockQuery);

            var dto = new InvoiceListDto { Id = 1, SerialNumber = "INV001" };
            _mockMapper.Setup(m => m.Map<InvoiceListDto>(It.IsAny<Invoice>())).Returns(dto);

            var result = await _invoiceService.GetByIdAsync(1);

            result.Should().NotBeNull();
            result.SerialNumber.Should().Be("INV001");
        }

        [Fact]
        public async Task GetByIdAsync_WhenInvoiceDoesNotExist()
        {
            var emptyInvoices = new List<Invoice>().BuildMock();
            _mockInvoiceRepo.Setup(x => x.GetQueryable()).Returns(emptyInvoices);

            Func<Task> action = async () => await _invoiceService.GetByIdAsync(1);

            await action.Should().ThrowAsync<BusinessException>()
                .Where(x => x.Message == ErrorKeys.InvoiceNotFound);
        }

        [Fact]
        public async Task CreateDraftInvoiceAsync()
        {
            var dto = new CreateInvoiceDto
            {
                CompanyId = 1,
                CurrentAccountId = 1,
                SerialNumber = "INV001",
                Date = DateTime.Now,
                Type = InvoiceType.Purchase,
                InvoiceDetails = new List<CreateInvoiceDetailDto>
                {
                    new CreateInvoiceDetailDto { StockId = 1, Quantity = 10, UnitPrice = 50 }
                }
            };

            await _invoiceService.CreateDraftInvoiceAsync(dto);

            _mockInvoiceRepo.Verify(x => x.AddAsync(It.Is<Invoice>(i => i.Status == InvoiceStatus.Draft && i.Type == InvoiceType.Purchase)), Times.Once);
            _mockInvoiceDetailRepo.Verify(x => x.AddAsync(It.IsAny<InvoiceDetail>()), Times.Once);
            _mockInvoiceDetailRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ApproveInvoiceAsync_WhenInvoiceNotDraft()
        {
            var invoice = new Invoice { Id = 1, Status = InvoiceStatus.Approved };
            _mockInvoiceRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(invoice);

            Func<Task> action = async () => await _invoiceService.ApproveInvoiceAsync(1);

            await action.Should().ThrowAsync<BusinessException>()
                .Where(x => x.Message == ErrorKeys.OnlyDraftInvoicesCanBeApproved);
        }

        [Fact]
        public async Task ApproveInvoiceAsync_WhenSalesAndInsufficientStock()
        {
            var invoice = new Invoice { Id = 1, Status = InvoiceStatus.Draft, Type = InvoiceType.Sales };
            _mockInvoiceRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(invoice);

            var details = new List<InvoiceDetail> { new InvoiceDetail { InvoiceId = 1, StockId = 1, Quantity = 10 } };
            _mockInvoiceDetailRepo.Setup(x => x.GetQueryable()).Returns(details.BuildMock());

            var stock = new Stock { Id = 1, Balance = 5 };
            _mockStockRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(stock);

            Func<Task> action = async () => await _invoiceService.ApproveInvoiceAsync(1);

            await action.Should().ThrowAsync<BusinessException>()
                .Where(x => x.Message == ErrorKeys.InsufficientStock);
        }

        [Fact]
        public async Task ApproveInvoiceAsync_WhenPurchase()
        {
            var invoice = new Invoice { Id = 1, Status = InvoiceStatus.Draft, Type = InvoiceType.Purchase, CurrentAccountId = 1 };
            _mockInvoiceRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(invoice);

            var details = new List<InvoiceDetail> { new InvoiceDetail { InvoiceId = 1, StockId = 1, Quantity = 10, UnitPrice = 100 } };
            _mockInvoiceDetailRepo.Setup(x => x.GetQueryable()).Returns(details.BuildMock());

            var stock = new Stock { Id = 1, Balance = 20 };
            _mockStockRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(stock);

            var currentAccount = new CurrentAccount { Id = 1, Balance = 500 };
            _mockCurrentAccountRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(currentAccount);

            await _invoiceService.ApproveInvoiceAsync(1);

            stock.Balance.Should().Be(30);
            currentAccount.Balance.Should().Be(1500);
            invoice.Status.Should().Be(InvoiceStatus.Approved);

            _mockStockTransRepo.Verify(x => x.AddAsync(It.Is<StockTrans>(st => st.Direction == TransactionType.In)), Times.Once);
            _mockStockRepo.Verify(x => x.Update(stock), Times.Once);
            _mockCurrentAccountRepo.Verify(x => x.Update(currentAccount), Times.Once);
            _mockInvoiceRepo.Verify(x => x.Update(invoice), Times.Once);
        }

        [Fact]
        public async Task SendInvoiceToIntegratorAsync_WhenNotApproved()
        {
            var invoice = new Invoice { Id = 1, Status = InvoiceStatus.Draft };
            _mockInvoiceRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(invoice);

            Func<Task> action = async () => await _invoiceService.SendInvoiceToIntegratorAsync(1);

            await action.Should().ThrowAsync<BusinessException>()
                .Where(x => x.Message == ErrorKeys.InvalidTransaction);
        }

        [Fact]
        public async Task SendInvoiceToIntegratorAsync_WhenApproved()
        {
            var invoice = new Invoice { Id = 1, Status = InvoiceStatus.Approved };
            _mockInvoiceRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(invoice);

            await _invoiceService.SendInvoiceToIntegratorAsync(1);

            invoice.Status.Should().Be(InvoiceStatus.Sent);
            _mockInvoiceRepo.Verify(x => x.Update(invoice), Times.Once);
            _mockInvoiceRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
    }
}