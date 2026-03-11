using AutoMapper;
using BusinessLayer.Abstract;
using DataAccessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.Pagination;
using EntityLayer.DTOs.Stock;
using EntityLayer.Entities.Domain;
using EntityLayer.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLayer.Concrete
{
    public class StockService : IStockService
    {
        private readonly IStockRepository _stockRepository;
        private readonly IMapper _mapper;

        public StockService(IStockRepository stockRepository, IMapper mapper)
        {
            _stockRepository = stockRepository;
            _mapper = mapper;
        }

        public async Task<PagedResponse<StockListDto>> GetAllStocksAsync(StockFilterDto filter, int companyId)
        {
            var validFilter = new StockFilterDto(filter.PageNumber, filter.PageSize);
            var query = _stockRepository.GetQueryable()
                                        .AsNoTracking()
                                        .Where(x => x.CompanyId == companyId);

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                query = query.Where(x => x.Name.Contains(filter.Name));
            }

            if (!string.IsNullOrWhiteSpace(filter.Code))
            {
                query = query.Where(x => x.Code.Contains(filter.Code));
            }

            if (filter.Unit.HasValue)
            {
                query = query.Where(x => x.Unit == filter.Unit.Value);
            }

            if (filter.MinBalance.HasValue)
            {
                query = query.Where(x => x.Balance >= filter.MinBalance.Value);
            }

            var totalRecords = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.CreateDate)
                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                .Take(validFilter.PageSize)
                .ToListAsync();

            var mappedData = _mapper.Map<IEnumerable<StockListDto>>(data);

            return new PagedResponse<StockListDto>(mappedData, totalRecords, validFilter.PageNumber, validFilter.PageSize);
        }

        public async Task<StockListDto> GetByIdAsync(int id)
        {
            var stock = await _stockRepository.GetByIdAsync(id);
            if (stock == null) throw new BusinessException(ErrorKeys.StockNotFound);

            return _mapper.Map<StockListDto>(stock);
        }

        public async Task AddAsync(CreateStockDto dto)
        {
            var isExist = await _stockRepository.AnyAsync(x => x.Code == dto.Code && x.CompanyId == dto.CompanyId);
            if (isExist) throw new BusinessException(ErrorKeys.StockAlreadyExists);

            var stock = _mapper.Map<Stock>(dto);

            dto.Balance = 0;

            await _stockRepository.AddAsync(stock);
        }

        public async Task UpdateAsync(UpdateStockDto dto)
        {
            var stock = await _stockRepository.GetByIdAsync(dto.Id);
            if (stock == null) throw new BusinessException(ErrorKeys.StockNotFound);

            _mapper.Map(dto, stock);
            _stockRepository.Update(stock);
        }

        public async Task DeleteAsync(int id)
        {
            var stock = await _stockRepository.GetByIdAsync(id);
            if (stock == null) throw new BusinessException(ErrorKeys.StockNotFound);

            _stockRepository.Delete(stock);
        }
    }
}