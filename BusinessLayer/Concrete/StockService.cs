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
        private readonly ICacheService _cacheService;

        public StockService(IStockRepository stockRepository, IMapper mapper, ICacheService cacheService)
        {
            _stockRepository = stockRepository;
            _mapper = mapper;
            _cacheService = cacheService; 
        }

        public async Task<PagedResponse<StockListDto>> GetAllStocksAsync(StockFilterDto filter, int companyId)
        {
            var cacheKey = $"Stocks_Company_{companyId}_Page_{filter.PageNumber}_Size_{filter.PageSize}_Name_{filter.Name}_Code_{filter.Code}_Unit_{filter.Unit}_MinBalance_{filter.MinBalance}";

            var cachedData = await _cacheService.GetAsync<PagedResponse<StockListDto>>(cacheKey);
            if (cachedData != null)
            {
                return cachedData; 
            }

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
            var response = new PagedResponse<StockListDto>(mappedData, totalRecords, validFilter.PageNumber, validFilter.PageSize);
            // --- SENİN ORİJİNAL KODUNUN SONU ---

            // 5. CACHE'E YAZMA: Bulunan sonucu 60 dakikalığına Redis'e kaydet
            await _cacheService.SetAsync(cacheKey, response, 60);

            return response;
        }

        public async Task<StockListDto> GetByIdAsync(int id)
        {
            // Tekil getirme işlemine de cache ekliyoruz
            var cacheKey = $"Stock_Single_{id}";
            var cachedData = await _cacheService.GetAsync<StockListDto>(cacheKey);
            if (cachedData != null) return cachedData;

            // Senin kodun
            var stock = await _stockRepository.GetByIdAsync(id);
            if (stock == null) throw new BusinessException(ErrorKeys.StockNotFound);
            var mappedData = _mapper.Map<StockListDto>(stock);

            // Redis'e kaydet
            await _cacheService.SetAsync(cacheKey, mappedData, 60);

            return mappedData;
        }

        public async Task AddAsync(CreateStockDto dto)
        {
            var isExist = await _stockRepository.AnyAsync(x => x.Code == dto.Code && x.CompanyId == dto.CompanyId);
            if (isExist) throw new BusinessException(ErrorKeys.StockAlreadyExists);

            var stock = _mapper.Map<Stock>(dto);

            // Not: Senin kodunda dto.Balance = 0; yazıyordu ama veritabanına giden nesne 'stock' olduğu için 
            // stock.Balance = 0; olması gerekiyor ki SQL'e 0 olarak gitsin. Onu düzelttim.
            stock.Balance = 0;

            await _stockRepository.AddAsync(stock);

            // 6. CACHE TEMİZLİĞİ: Yeni stok eklendi, o şirketin eski liste cache'lerini sil
            await _cacheService.RemoveByPatternAsync($"Stocks_Company_{dto.CompanyId}*");
        }

        public async Task UpdateAsync(UpdateStockDto dto)
        {
            var stock = await _stockRepository.GetByIdAsync(dto.Id);
            if (stock == null) throw new BusinessException(ErrorKeys.StockNotFound);

            _mapper.Map(dto, stock);
            _stockRepository.Update(stock);

            // 7. CACHE TEMİZLİĞİ: Güncellendiği için detay ve liste cache'lerini sil
            await _cacheService.RemoveAsync($"Stock_Single_{dto.Id}");
            await _cacheService.RemoveByPatternAsync($"Stocks_Company_{stock.CompanyId}*");
        }

        public async Task DeleteAsync(int id)
        {
            var stock = await _stockRepository.GetByIdAsync(id);
            if (stock == null) throw new BusinessException(ErrorKeys.StockNotFound);

            _stockRepository.Delete(stock);

            // 8. CACHE TEMİZLİĞİ: Silindiği için detay ve liste cache'lerini sil
            await _cacheService.RemoveAsync($"Stock_Single_{id}");
            await _cacheService.RemoveByPatternAsync($"Stocks_Company_{stock.CompanyId}*");
        }
    }
}