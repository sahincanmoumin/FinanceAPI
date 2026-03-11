using AutoMapper;
using BusinessLayer.Abstract;
using DataAccessLayer.Abstract;
using EntityLayer.DTOs.Pagination;
using EntityLayer.DTOs.StockTrans;
using EntityLayer.Entities.Domain;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLayer.Concrete
{
    public class StockTransService : IStockTransService
    {
        private readonly IGenericRepository<StockTrans> _stockTransRepository;
        private readonly IMapper _mapper;

        public StockTransService(IGenericRepository<StockTrans> stockTransRepository, IMapper mapper)
        {
            _stockTransRepository = stockTransRepository;
            _mapper = mapper;
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