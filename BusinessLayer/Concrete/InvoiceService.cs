using AutoMapper;
using BusinessLayer.Abstract;
using DataAccessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.Invoice;
using EntityLayer.DTOs.Pagination;
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
        private readonly IStockRepository _stockRepository;
        private readonly ICurrentAccountRepository _currentAccountRepository;
        private readonly IStockTransRepository _stockTransRepository;
        private readonly IMapper _mapper;
        private readonly IStockTransService _stockTransService;

        public InvoiceService(
            IInvoiceRepository invoiceRepository,
            IInvoiceDetailRepository invoiceDetailRepository,
            IStockRepository stockRepository,
            ICurrentAccountRepository currentAccountRepository,
            IStockTransRepository stockTransRepository,
            IMapper mapper,
            IStockTransService stockTransService)
        {
            _invoiceRepository = invoiceRepository;
            _invoiceDetailRepository = invoiceDetailRepository;
            _stockRepository = stockRepository;
            _currentAccountRepository = currentAccountRepository;
            _stockTransRepository = stockTransRepository;
            _mapper = mapper;
            _stockTransService = stockTransService;
        }

        public async Task<PagedResponse<InvoiceListDto>> GetAllInvoicesAsync(InvoiceFilterDto filter, int companyId)
        {
            var validFilter = new InvoiceFilterDto(filter.PageNumber, filter.PageSize);

            var query = _invoiceRepository.GetQueryable()
                                          .AsNoTracking()
                                          .Include(x => x.InvoiceDetails)
                                          .Where(x => x.CompanyId == companyId);

            if (!string.IsNullOrWhiteSpace(filter.SerialNumber))
                query = query.Where(x => x.SerialNumber.Contains(filter.SerialNumber));

            if (filter.Status.HasValue)
                query = query.Where(x => x.Status == filter.Status.Value);

            if (filter.StockId.HasValue)
            {
                query = query.Where(x => x.InvoiceDetails.Any(d => d.StockId == filter.StockId.Value));
            }

            if (filter.MinUnitPrice.HasValue)
            {
                query = query.Where(x => x.InvoiceDetails.Any(d => d.UnitPrice >= filter.MinUnitPrice.Value));
            }

            if (filter.StartDate.HasValue)
                query = query.Where(x => x.Date >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
            {
                var endOfDay = filter.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.Date <= endOfDay);
            }
            if (filter.Type.HasValue)
            {
                query = query.Where(x => x.Type == filter.Type.Value);
            }

            var totalRecords = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.Date)
                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                .Take(validFilter.PageSize)
                .ToListAsync();

            var mappedData = _mapper.Map<IEnumerable<InvoiceListDto>>(data);
            return new PagedResponse<InvoiceListDto>(mappedData, totalRecords, validFilter.PageNumber, validFilter.PageSize);
        }

        public async Task<InvoiceListDto> GetByIdAsync(int id)
        {
            var invoice = await _invoiceRepository.GetQueryable()
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
                SerialNumber = dto.SerialNumber,
                Ettn = Guid.NewGuid(),
                Date = dto.Date,
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

        public async Task ApproveInvoiceAsync(int invoiceId)
        {
            using var transaction = await _invoiceRepository.BeginTransactionAsync();

            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (invoice == null) throw new BusinessException(ErrorKeys.InvoiceNotFound);

            if (invoice.Status != InvoiceStatus.Draft)
                throw new BusinessException(ErrorKeys.InvoiceNotDraft);

            var details = await _invoiceDetailRepository.GetQueryable().Where(x => x.InvoiceId == invoiceId).ToListAsync();

            decimal totalInvoiceAmount = 0;
            TransactionType direction = invoice.Type == InvoiceType.Purchase ? TransactionType.In : TransactionType.Out;

            foreach (var detail in details)
            {
                await _stockTransService.ProcessStockActionAsync(
                    invoice.CompanyId,
                    detail.StockId,
                    detail.Quantity,
                    detail.UnitPrice,
                    direction
                );

                totalInvoiceAmount += (detail.Quantity * detail.UnitPrice);
            }

            var currentAccount = await _currentAccountRepository.GetByIdAsync(invoice.CurrentAccountId);
            if (currentAccount != null)
            {
                currentAccount.Balance += totalInvoiceAmount;
                _currentAccountRepository.Update(currentAccount);
            }

            invoice.Status = InvoiceStatus.Approved;
            _invoiceRepository.Update(invoice);

            await _invoiceRepository.SaveChangesAsync();

            await transaction.CommitAsync();
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
        }
    }
}