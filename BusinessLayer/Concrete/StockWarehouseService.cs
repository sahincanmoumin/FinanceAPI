using AutoMapper;
using BusinessLayer.Abstract;
using DataAccessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.StockWarehouse;
using EntityLayer.Entities;
using EntityLayer.Entities.Domain;
using EntityLayer.Entities.Enums;
using EntityLayer.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLayer.Concrete
{
    public class StockWarehouseService : IStockWarehouseService
    {
        private readonly IGenericRepository<StockWarehouse> _swRepository;
        private readonly IGenericRepository<Stock> _stockRepository;
        private readonly IMapper _mapper;

        public StockWarehouseService(
            IGenericRepository<StockWarehouse> swRepository,
            IGenericRepository<Stock> stockRepository,
            IMapper mapper)
        {
            _swRepository = swRepository;
            _stockRepository = stockRepository;
            _mapper = mapper;
        }

        public async Task UpdateTransferBalanceAsync(int stockId, int fromWarehouseId, int toWarehouseId, decimal quantity)
        {
            await AdjustWarehouseBalance(stockId, fromWarehouseId, quantity, ReceiptType.Output);
            await AdjustWarehouseBalance(stockId, toWarehouseId, quantity, ReceiptType.Input);
        }
        private void ValidateWarehouseStock(StockWarehouse existingRecord, decimal quantity, ReceiptType type)
        {
            if (type == ReceiptType.Output || type == ReceiptType.Transfer)
            {
                if (existingRecord == null)
                    throw new BusinessException(ErrorKeys.StockNotFound);

                if (existingRecord.Quantity < quantity)
                    throw new BusinessException(ErrorKeys.InsufficientStock);
            }
        }
        public async Task UpdateBalanceAsync(int stockId, int warehouseId, decimal quantity, ReceiptType type)
        {
            await AdjustWarehouseBalance(stockId, warehouseId, quantity, type);

            var stock = await ValidateStockAndGetAsync(stockId);

            if (type == ReceiptType.Input)
                stock.Balance += quantity;
            else if (type == ReceiptType.Output)
                stock.Balance -= quantity;

            _stockRepository.Update(stock);
        }


        public async Task<List<StockWarehouseListDto>> GetAllStockStatusAsync(StockWarehouseFilterDto filter)
        {
            var query = _swRepository.GetQueryable()
                .Include(x => x.Stock)
                .Include(x => x.Warehouse)
                .AsNoTracking();

            query = query.Where(x => x.Stock.CompanyId == filter.CompanyId);

            if (filter.WarehouseId.HasValue)
                query = query.Where(x => x.WarehouseId == filter.WarehouseId.Value);

            if (filter.StockId.HasValue)
                query = query.Where(x => x.StockId == filter.StockId.Value);

            var data = await query.ToListAsync();

            return _mapper.Map<List<StockWarehouseListDto>>(data);
        }

        private async Task AdjustWarehouseBalance(int stockId, int warehouseId, decimal quantity, ReceiptType type)
        {
            var existingRecord = await _swRepository.GetQueryable()
                .FirstOrDefaultAsync(x => x.StockId == stockId && x.WarehouseId == warehouseId);

            ValidateWarehouseStock(existingRecord, quantity, type);

            if (existingRecord == null)
            {
                await _swRepository.AddAsync(new StockWarehouse
                {
                    StockId = stockId,
                    WarehouseId = warehouseId,
                    Quantity = quantity
                });
            }
            else
            {
                if (type == ReceiptType.Input)
                    existingRecord.Quantity += quantity;
                else
                    existingRecord.Quantity -= quantity;

                _swRepository.Update(existingRecord);
            }
        }

        private async Task<Stock> ValidateStockAndGetAsync(int stockId)
        {
            var stock = await _stockRepository.GetByIdAsync(stockId);
            if (stock == null)
                throw new BusinessException(ErrorKeys.StockNotFound);

            return stock;
        }
    }
}