using AutoMapper;
using BusinessLayer.Abstract;
using DataAccessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.Invoice;
using EntityLayer.DTOs.Pagination;
using EntityLayer.DTOs.StockReceipt;
using EntityLayer.DTOs.StockReceiptDetail;
using EntityLayer.Entities.Domain;
using EntityLayer.Entities.Enums;
using EntityLayer.Exceptions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLayer.Concrete
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IInvoiceDetailRepository _invoiceDetailRepository;
        private readonly ICurrentAccountRepository _currentAccountRepository;
        private readonly IStockReceiptService _stockReceiptService;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;

        public InvoiceService(
            IInvoiceRepository invoiceRepository,
            IInvoiceDetailRepository invoiceDetailRepository,
            ICurrentAccountRepository currentAccountRepository,
            IStockReceiptService stockReceiptService,
            ICacheService cacheService,
            IMapper mapper)
        {
            _invoiceRepository = invoiceRepository;
            _invoiceDetailRepository = invoiceDetailRepository;
            _currentAccountRepository = currentAccountRepository;
            _stockReceiptService = stockReceiptService;
            _cacheService = cacheService;
            _mapper = mapper;
        }

        public async Task ApproveInvoiceAsync(int invoiceId)
        {
            using var transaction = await _invoiceRepository.BeginTransactionAsync();

            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (invoice == null || invoice.Status != InvoiceStatus.Draft)
                throw new BusinessException("Fatura bulunamadı veya onay süreci için uygun değil.");

            var details = await _invoiceDetailRepository.GetQueryable()
                .Where(x => x.InvoiceId == invoiceId).ToListAsync();

            if (!details.Any()) throw new BusinessException("Faturaya ait detay bulunamadı.");

            var receiptDto = new CreateStockReceiptDto
            {
                CompanyId = invoice.CompanyId,
                WarehouseId = invoice.WarehouseId,
                SerialNumber = invoice.SerialNumber,
                Type = invoice.Type == InvoiceType.Purchase ? ReceiptType.Input : ReceiptType.Output,
                CurrentAccountId = invoice.CurrentAccountId,
                Details = details.Select(d => new CreateStockReceiptDetailDto
                {
                    StockId = d.StockId,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice
                }).ToList()
            };

            var createdReceipt = await _stockReceiptService.AddAsync(receiptDto);
            await _stockReceiptService.ApproveAsync(createdReceipt.Id, true);

            decimal totalAmount = details.Sum(d => d.Quantity * d.UnitPrice);
            var currentAccount = await _currentAccountRepository.GetByIdAsync(invoice.CurrentAccountId);

            if (currentAccount != null)
            {
                if (invoice.Type == InvoiceType.Sales)
                    currentAccount.Balance += totalAmount;
                else
                    currentAccount.Balance -= totalAmount;

                _currentAccountRepository.Update(currentAccount);
            }

            invoice.Status = InvoiceStatus.Approved;
            _invoiceRepository.Update(invoice);

            await _invoiceRepository.SaveChangesAsync();

            await _cacheService.RemoveAsync($"CurrentAccount_Single_{invoice.CurrentAccountId}");
            await _cacheService.RemoveByPatternAsync($"CurrentAccounts_Company_{invoice.CompanyId}*");
            await _cacheService.RemoveByPatternAsync($"Stocks_Company_{invoice.CompanyId}*");

            await transaction.CommitAsync();
        }

        public async Task<PagedResponse<InvoiceListDto>> GetAllInvoicesAsync(InvoiceFilterDto filter, int companyId)
        {
            var validFilter = new InvoiceFilterDto(filter.PageNumber, filter.PageSize);

            var query = _invoiceRepository.GetQueryable()
                                          .AsNoTracking()
                                          .Include(x => x.Warehouse)
                                          .Include(x => x.CurrentAccount)
                                          .Include(x => x.InvoiceDetails)
                                          .Where(x => x.CompanyId == companyId);

            if (!string.IsNullOrWhiteSpace(filter.SerialNumber))
                query = query.Where(x => x.SerialNumber.Contains(filter.SerialNumber));

            if (filter.Status.HasValue)
                query = query.Where(x => x.Status == filter.Status.Value);

            if (filter.StockId.HasValue)
                query = query.Where(x => x.InvoiceDetails.Any(d => d.StockId == filter.StockId.Value));

            if (filter.MinUnitPrice.HasValue)
                query = query.Where(x => x.InvoiceDetails.Any(d => d.UnitPrice >= filter.MinUnitPrice.Value));

            if (filter.StartDate.HasValue)
                query = query.Where(x => x.CreateDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
            {
                var endOfDay = filter.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.CreateDate <= endOfDay);
            }

            if (filter.Type.HasValue)
                query = query.Where(x => x.Type == filter.Type.Value);

            var totalRecords = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.CreateDate)
                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                .Take(validFilter.PageSize)
                .ToListAsync();

            var mappedData = _mapper.Map<IEnumerable<InvoiceListDto>>(data);
            return new PagedResponse<InvoiceListDto>(mappedData, totalRecords, validFilter.PageNumber, validFilter.PageSize);
        }

        public async Task<InvoiceListDto> GetByIdAsync(int id)
        {
            var invoice = await _invoiceRepository.GetQueryable()
                                 .Include(x => x.Warehouse)
                                 .Include(x => x.CurrentAccount)
                                 .Include(x => x.InvoiceDetails)
                                 .ThenInclude(d => d.Stock)
                                 .FirstOrDefaultAsync(x => x.Id == id);

            if (invoice == null) throw new BusinessException(ErrorKeys.InvoiceNotFound);

            return _mapper.Map<InvoiceListDto>(invoice);
        }

        public async Task<InvoiceListDto> CreateDraftInvoiceAsync(CreateInvoiceDto dto)
        {
            var invoice = new Invoice
            {
                CompanyId = dto.CompanyId,
                CurrentAccountId = dto.CurrentAccountId,
                WarehouseId = dto.WarehouseId,
                SerialNumber = dto.SerialNumber,
                Ettn = Guid.NewGuid(),
                Status = InvoiceStatus.Draft,
                Type = dto.Type,
                InvoiceDetails = new List<InvoiceDetail>()
            };

            await _invoiceRepository.AddAsync(invoice);

            foreach (var detailDto in dto.InvoiceDetails)
            {
                var detail = new InvoiceDetail
                {
                    InvoiceId = invoice.Id,
                    StockId = detailDto.StockId,
                    Quantity = detailDto.Quantity,
                    UnitPrice = detailDto.UnitPrice
                };
                await _invoiceDetailRepository.AddAsync(detail);
                invoice.InvoiceDetails.Add(detail);
            }
            await _invoiceDetailRepository.SaveChangesAsync();

            return _mapper.Map<InvoiceListDto>(invoice);
        }

        public async Task SendInvoiceToIntegratorAsync(int invoiceId)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);

            if (invoice.Status != InvoiceStatus.Approved)
                throw new BusinessException(ErrorKeys.InvalidTransaction);

            invoice.Status = InvoiceStatus.Sent;
            _invoiceRepository.Update(invoice);

            await _invoiceRepository.SaveChangesAsync();
        }

        public async Task DeleteDraftInvoiceAsync(int invoiceId)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (invoice == null) throw new BusinessException(ErrorKeys.InvoiceNotFound);

            if (invoice.Status != InvoiceStatus.Draft)
                throw new BusinessException(ErrorKeys.InvoiceNotDraft);

            var details = await _invoiceDetailRepository.GetQueryable()
                .Where(x => x.InvoiceId == invoiceId)
                .ToListAsync();

            foreach (var detail in details)
            {
                _invoiceDetailRepository.Delete(detail);
            }

            _invoiceRepository.Delete(invoice);
            await _invoiceRepository.SaveChangesAsync();
        }
    }
}