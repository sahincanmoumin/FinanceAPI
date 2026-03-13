using AutoMapper;
using BusinessLayer.Abstract;
using DataAccessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.Pagination;
using EntityLayer.DTOs.StockTrans;
using EntityLayer.Entities.Domain;
using EntityLayer.Entities.Enums;
using EntityLayer.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLayer.Concrete
{
    public class StockTransService : IStockTransService
    {
        private readonly IGenericRepository<StockTrans> _stockTransRepository;
        private readonly IGenericRepository<Stock> _stockRepository; 
        private readonly IMapper _mapper;

        public StockTransService(IGenericRepository<StockTrans> stockTransRepository, IGenericRepository<Stock> stockRepository, IMapper mapper)
        {
            _stockTransRepository = stockTransRepository;
            _stockRepository = stockRepository;
            _mapper = mapper;
        }
        public async Task ProcessStockActionAsync(int companyId, int stockId, decimal quantity, decimal unitPrice, TransactionType direction)
        {
            var stock = await _stockRepository.GetByIdAsync(stockId);
            if (stock == null) throw new BusinessException(ErrorKeys.StockNotFound);

            if (direction == TransactionType.Out && stock.Balance < quantity)
                throw new BusinessException(ErrorKeys.InsufficientStock);

            var stockTrans = new StockTrans
            {
                CompanyId = companyId,
                StockId = stockId,
                Date = DateTime.Now,
                Quantity = quantity,
                UnitPrice = unitPrice,
                Direction = direction
            };
            await _stockTransRepository.AddAsync(stockTrans);

            if (direction == TransactionType.In)
                stock.Balance += quantity;
            else
                stock.Balance -= quantity;

            _stockRepository.Update(stock);
        }

        public async Task<PagedResponse<StockTransListDto>> GetTransactionsByStockIdAsync(int stockId, StockTransFilterDto filter)
        {
            var validFilter = new StockTransFilterDto(filter.PageNumber, filter.PageSize);

            var query = _stockTransRepository.GetQueryable()
                .AsNoTracking()
                .Where(x => x.StockId == stockId);

            if (filter.Direction.HasValue)
                query = query.Where(x => x.Direction == filter.Direction.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(x => x.Date >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
            {
                var endOfDay = filter.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.Date <= endOfDay);
            }

            if (filter.MinQuantity.HasValue)
                query = query.Where(x => x.Quantity >= filter.MinQuantity.Value);

            if (filter.MaxQuantity.HasValue)
                query = query.Where(x => x.Quantity <= filter.MaxQuantity.Value);

            if (filter.MinUnitPrice.HasValue)
                query = query.Where(x => x.UnitPrice >= filter.MinUnitPrice.Value);

            if (filter.MaxUnitPrice.HasValue)
                query = query.Where(x => x.UnitPrice <= filter.MaxUnitPrice.Value);

            var totalRecords = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.Date)
                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                .Take(validFilter.PageSize)
                .ToListAsync();

            var mappedData = _mapper.Map<IEnumerable<StockTransListDto>>(data);

            return new PagedResponse<StockTransListDto>(mappedData, totalRecords, validFilter.PageNumber, validFilter.PageSize);
        }
    }
}