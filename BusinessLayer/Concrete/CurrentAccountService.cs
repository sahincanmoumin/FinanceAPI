using AutoMapper;
using BusinessLayer.Abstract;
using DataAccessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.CurrentAccount;
using EntityLayer.DTOs.Pagination;
using EntityLayer.Entities.Domain;
using EntityLayer.Entities.Enums;
using EntityLayer.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLayer.Concrete
{
    public class CurrentAccountService : ICurrentAccountService
    {
        private readonly ICurrentAccountRepository _currentAccountRepository;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public CurrentAccountService(ICurrentAccountRepository currentAccountRepository, IMapper mapper, ICacheService cacheService)
        {
            _currentAccountRepository = currentAccountRepository;
            _mapper = mapper;
            _cacheService = cacheService; 
        }

        public async Task<CurrentAccountListDto> GetByIdAsync(int id)
        {
            var cacheKey = $"CurrentAccount_Single_{id}";
            var cachedData = await _cacheService.GetAsync<CurrentAccountListDto>(cacheKey);
            if (cachedData != null) {
                return cachedData;
            }

            var account = await _currentAccountRepository.GetByIdAsync(id);
            if (account == null) throw new BusinessException(ErrorKeys.CurrentAccountNotFound);

            var mappedData = _mapper.Map<CurrentAccountListDto>(account);


            await _cacheService.SetAsync(cacheKey, mappedData, 60);

            return mappedData;
        }

        public async Task AddAsync(CreateCurrentAccountDto dto)
        {
            var isExist = await _currentAccountRepository.AnyAsync(x => x.Code == dto.Code && x.CompanyId == dto.CompanyId);
            if (isExist) throw new BusinessException(ErrorKeys.CurrentAccountAlreadyExists);

            if (!System.Enum.IsDefined(typeof(AccountType), dto.Type))
                throw new BusinessException(ErrorKeys.InvalidAccountType);

            var account = _mapper.Map<CurrentAccount>(dto);
            if (account.Balance < 0) account.Balance = 0;

            await _currentAccountRepository.AddAsync(account);

            // yeni veriden sonra cache silme kısmii
            await _cacheService.RemoveByPatternAsync($"CurrentAccounts_Company_{dto.CompanyId}*");
        }

        public async Task<PagedResponse<CurrentAccountListDto>> GetAllCurrentAccountsAsync(CurrentAccountFilterDto filter, int companyId)
        {
            var cacheKey = $"CurrentAccounts_Company_{companyId}_Page_{filter.PageNumber}_Size_{filter.PageSize}_Name_{filter.Name}_Type_{filter.Type}_Balance_{filter.balance}";

            var cachedData = await _cacheService.GetAsync<PagedResponse<CurrentAccountListDto>>(cacheKey);
            if (cachedData != null)
            {
                return cachedData;
            }

            var validFilter = new CurrentAccountFilterDto(filter.PageNumber, filter.PageSize);
            var query = _currentAccountRepository.GetQueryable()
                                                 .AsNoTracking()
                                                 .Where(x => x.CompanyId == companyId);

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                query = query.Where(x => x.Name.Contains(filter.Name));
            }

            if (filter.Type.HasValue)
            {
                query = query.Where(x => x.Type == filter.Type.Value);
            }
            if (filter.balance.HasValue)
            {
                query = query.Where(x => x.Balance <= filter.balance.Value);
            }

            var totalRecords = await query.CountAsync();

            var accounts = await query
                .OrderByDescending(x => x.CreateDate)
                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                .Take(validFilter.PageSize)
                .ToListAsync();

            var mappedAccounts = _mapper.Map<IEnumerable<CurrentAccountListDto>>(accounts);
            var response = new PagedResponse<CurrentAccountListDto>(mappedAccounts, totalRecords, validFilter.PageNumber, validFilter.PageSize);

            await _cacheService.SetAsync(cacheKey, response, 60);

            return response;
        }

        public async Task UpdateAsync(UpdateCurrentAccountDto dto)
        {
            var account = await _currentAccountRepository.GetByIdAsync(dto.Id);
            if (account == null) throw new BusinessException(ErrorKeys.CurrentAccountNotFound);

            _mapper.Map(dto, account);
            _currentAccountRepository.Update(account);

            //güncelleme sonrası temizlik
            await _cacheService.RemoveAsync($"CurrentAccount_Single_{dto.Id}");
            await _cacheService.RemoveByPatternAsync($"CurrentAccounts_Company_{account.CompanyId}*");
        }

        public async Task DeleteAsync(int id)
        {
            var account = await _currentAccountRepository.GetByIdAsync(id);
            if (account == null) throw new BusinessException(ErrorKeys.CurrentAccountNotFound);

            _currentAccountRepository.Delete(account);

            //temizlik sonrası silme
            await _cacheService.RemoveAsync($"CurrentAccount_Single_{id}");
            await _cacheService.RemoveByPatternAsync($"CurrentAccounts_Company_{account.CompanyId}*");
        }
    }
}