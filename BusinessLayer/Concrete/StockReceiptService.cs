using AutoMapper;
using BusinessLayer.Abstract;
using DataAccessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.Pagination;
using EntityLayer.DTOs.StockReceipt;
using EntityLayer.Entities.Domain;
using EntityLayer.Entities.Enums;
using EntityLayer.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLayer.Concrete
{
    public class StockReceiptService : IStockReceiptService
    {
        private readonly IGenericRepository<StockReceipt> _receiptRepository;
        private readonly IStockTransService _stockTransService;
        private readonly IStockWarehouseService _stockWarehouseService;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;

        public StockReceiptService(
            IGenericRepository<StockReceipt> receiptRepository,
            IStockTransService stockTransService,
            IStockWarehouseService stockWarehouseService,
            ICacheService cacheService,
            IMapper mapper)
        {
            _receiptRepository = receiptRepository;
            _stockTransService = stockTransService;
            _stockWarehouseService = stockWarehouseService;
            _cacheService = cacheService;
            _mapper = mapper;
        }

        private void ValidateIsDraft(ReceiptStatus status, string errorKey)
        {
            if (status != ReceiptStatus.Draft)
                throw new BusinessException(errorKey);
        }

        private void ValidateTransferTarget(ReceiptType type, int? targetWarehouseId)
        {
            if (type == ReceiptType.Transfer && !targetWarehouseId.HasValue)
                throw new BusinessException(ErrorKeys.TransferReceiptTargetWarehouseRequired);
        }

        public async Task ApproveAsync(int id, bool isNested = false)
        {
            var transaction = isNested ? null : await _receiptRepository.BeginTransactionAsync();

            try
            {
                var receipt = await _receiptRepository.GetQueryable()
                    .Include(x => x.Details)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (receipt == null) throw new BusinessException(ErrorKeys.ReceiptNotFound);

                ValidateIsDraft(receipt.Status, ErrorKeys.OnlyDraftReceiptsCanBeApproved);

                foreach (var detail in receipt.Details)
                {
                    if (receipt.Type == ReceiptType.Transfer)
                    {
                        ValidateTransferTarget(receipt.Type, receipt.TargetWarehouseId);

                        await _stockTransService.ProcessStockActionAsync(receipt.CompanyId, detail.StockId, detail.Quantity, detail.UnitPrice, TransactionType.Out, receipt.WarehouseId, receipt.Id);
                        await _stockTransService.ProcessStockActionAsync(receipt.CompanyId, detail.StockId, detail.Quantity, detail.UnitPrice, TransactionType.In, receipt.TargetWarehouseId.Value, receipt.Id);
                        await _stockWarehouseService.UpdateTransferBalanceAsync(detail.StockId, receipt.WarehouseId, receipt.TargetWarehouseId.Value, detail.Quantity);
                    }
                    else
                    {
                        var direction = receipt.Type == ReceiptType.Output ? TransactionType.Out : TransactionType.In;
                        await _stockTransService.ProcessStockActionAsync(receipt.CompanyId, detail.StockId, detail.Quantity, detail.UnitPrice, direction, receipt.WarehouseId, receipt.Id);
                        await _stockWarehouseService.UpdateBalanceAsync(detail.StockId, receipt.WarehouseId, detail.Quantity, receipt.Type);
                    }
                }

                receipt.Status = ReceiptStatus.Approved;
                _receiptRepository.Update(receipt);
                await _receiptRepository.SaveChangesAsync();

                await _cacheService.RemoveByPatternAsync($"Stocks_Company_{receipt.CompanyId}*");

                if (transaction != null) await transaction.CommitAsync();
            }
            catch
            {
                if (transaction != null) await transaction.RollbackAsync();
                throw;
            }
            finally
            {
                if (transaction != null) transaction.Dispose();
            }
        }

        public async Task<PagedResponse<StockReceiptListDto>> GetAllAsync(StockReceiptFilterDto filter)
        {
            var validFilter = new StockReceiptFilterDto(filter.PageNumber, filter.PageSize);
            var query = _receiptRepository.GetQueryable()
                .AsNoTracking()
                .Include(x => x.Warehouse)
                .Include(x => x.TargetWarehouse)
                .Include(x => x.CurrentAccount)
                .Include(x => x.Details)
                .ThenInclude(d => d.Stock)
                .OrderByDescending(x => x.CreateDate);

            var totalRecords = await query.CountAsync();
            var pagedData = await query
                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                .Take(validFilter.PageSize)
                .ToListAsync();

            var mappedData = _mapper.Map<List<StockReceiptListDto>>(pagedData);
            return new PagedResponse<StockReceiptListDto>(mappedData, totalRecords, validFilter.PageNumber, validFilter.PageSize);
        }

        public async Task<StockReceiptListDto> GetByIdAsync(int id)
        {
            var receipt = await _receiptRepository.GetQueryable()
                .AsNoTracking()
                .Include(x => x.Warehouse)
                .Include(x => x.TargetWarehouse)
                .Include(x => x.CurrentAccount)
                .Include(x => x.Details)
                .ThenInclude(d => d.Stock)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (receipt == null) throw new BusinessException(ErrorKeys.ReceiptNotFound);

            return _mapper.Map<StockReceiptListDto>(receipt);
        }

        public async Task<StockReceiptListDto> AddAsync(CreateStockReceiptDto dto)
        {
            var receipt = _mapper.Map<StockReceipt>(dto);
            receipt.Status = ReceiptStatus.Draft;

            await _receiptRepository.AddAsync(receipt);
            await _receiptRepository.SaveChangesAsync();

            return await GetByIdAsync(receipt.Id);
        }

        public async Task<StockReceiptListDto> UpdateAsync(UpdateStockReceiptDto dto)
        {
            var existingReceipt = await _receiptRepository.GetByIdAsync(dto.Id);
            if (existingReceipt == null) throw new BusinessException(ErrorKeys.ReceiptNotFound);

            ValidateIsDraft(existingReceipt.Status, ErrorKeys.ReceiptCannotBeModified);

            _mapper.Map(dto, existingReceipt);
            _receiptRepository.Update(existingReceipt);
            await _receiptRepository.SaveChangesAsync();

            return await GetByIdAsync(existingReceipt.Id);
        }

        public async Task DeleteAsync(int id)
        {
            var receipt = await _receiptRepository.GetByIdAsync(id);
            if (receipt == null) throw new BusinessException(ErrorKeys.ReceiptNotFound);

            ValidateIsDraft(receipt.Status, ErrorKeys.StockReceiptNotDraft);

            _receiptRepository.Delete(receipt);
            await _receiptRepository.SaveChangesAsync();
        }
    }
}