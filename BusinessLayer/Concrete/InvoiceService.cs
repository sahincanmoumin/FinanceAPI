using AutoMapper;
using BusinessLayer.Abstract;
using DataAccessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.Invoice;
using EntityLayer.DTOs.Pagination;
using EntityLayer.Entities.Domain;
using EntityLayer.Entities.Enums;
using EntityLayer.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private readonly IValidator<CreateInvoiceDto> _createValidator;

        public InvoiceService(
            IInvoiceRepository invoiceRepository,
            IInvoiceDetailRepository invoiceDetailRepository,
            IStockRepository stockRepository,
            ICurrentAccountRepository currentAccountRepository,
            IStockTransRepository stockTransRepository,
            IMapper mapper,
            IValidator<CreateInvoiceDto> createValidator)
        {
            _invoiceRepository = invoiceRepository;
            _invoiceDetailRepository = invoiceDetailRepository;
            _stockRepository = stockRepository;
            _currentAccountRepository = currentAccountRepository;
            _stockTransRepository = stockTransRepository;
            _mapper = mapper;
            _createValidator = createValidator;
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
            //detail için olan filtree
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

        public async Task CreateDraftInvoiceAsync(CreateInvoiceDto dto)
        {
            await _createValidator.ValidateAndThrowAsync(dto);

            var invoice = new Invoice
            {
                CompanyId = dto.CompanyId,
                CurrentAccountId = dto.CurrentAccountId,
                SerialNumber = dto.SerialNumber,
                Ettn = Guid.NewGuid(),
                Date = dto.Date,
                Status = InvoiceStatus.Draft,
                Type = dto.Type
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
            }
            await _invoiceDetailRepository.SaveChangesAsync();
        }

        public async Task ApproveInvoiceAsync(int invoiceId)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (invoice == null) throw new BusinessException(ErrorKeys.InvoiceNotFound);

            if (invoice.Status != InvoiceStatus.Draft)
                throw new BusinessException(ErrorKeys.OnlyDraftInvoicesCanBeApproved);

            var details = await _invoiceDetailRepository.GetQueryable().Where(x => x.InvoiceId == invoiceId).ToListAsync();

            decimal totalInvoiceAmount = 0;
            bool isPurchase = invoice.Type == InvoiceType.Purchase;

            foreach (var detail in details)
            {
                var stock = await _stockRepository.GetByIdAsync(detail.StockId);
                if (stock == null) throw new BusinessException(ErrorKeys.StockNotFound);


                if (!isPurchase && stock.Balance < detail.Quantity)
                {
                    throw new BusinessException(ErrorKeys.InsufficientStock);
                }

                var stockTrans = new StockTrans
                {
                    CompanyId = invoice.CompanyId,
                    StockId = stock.Id,
                    Date = DateTime.Now,
                    Quantity = detail.Quantity,
                    UnitPrice = detail.UnitPrice,
                    Direction = isPurchase ? TransactionType.In : TransactionType.Out
                };
                await _stockTransRepository.AddAsync(stockTrans);

                if (isPurchase)
                    stock.Balance += detail.Quantity;
                else
                    stock.Balance -= detail.Quantity; 
                _stockRepository.Update(stock);

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