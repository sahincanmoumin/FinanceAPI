using AutoMapper;
using BusinessLayer.Abstract;
using DataAccessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.CurrentAccount;
using EntityLayer.DTOs.Pagination;
using EntityLayer.Entities.Domain;
using EntityLayer.Entities.Enums;
using EntityLayer.Exceptions;
using FluentValidation;
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
        private readonly IValidator<CreateCurrentAccountDto> _createValidator;
        private readonly IValidator<UpdateCurrentAccountDto> _updateValidator;

        public CurrentAccountService(
            ICurrentAccountRepository currentAccountRepository,
            IMapper mapper,
            ICacheService cacheService,
            IValidator<CreateCurrentAccountDto> createValidator,
            IValidator<UpdateCurrentAccountDto> updateValidator)
        {
            _currentAccountRepository = currentAccountRepository;
            _mapper = mapper;
            _cacheService = cacheService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        private async Task ValidateForCreateAsync(CreateCurrentAccountDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new BusinessException(errors);
            }

            var isExist = await _currentAccountRepository.AnyAsync(x => x.Code == dto.Code && x.CompanyId == dto.CompanyId);
            if (isExist) throw new BusinessException(ErrorKeys.CurrentAccountAlreadyExists);
        }

        private async Task ValidateForUpdateAsync(UpdateCurrentAccountDto dto)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new BusinessException(errors);
            }

            var isExist = await _currentAccountRepository.AnyAsync(x => x.Code == dto.Code && x.CompanyId == dto.CompanyId && x.Id != dto.Id);
            if (isExist) throw new BusinessException(ErrorKeys.CurrentAccountAlreadyExists);
        }

        public async Task<CurrentAccountListDto> AddAsync(CreateCurrentAccountDto dto)
        {
            await ValidateForCreateAsync(dto);

            var account = _mapper.Map<CurrentAccount>(dto);
            if (account.Balance < 0) account.Balance = 0;

            await _currentAccountRepository.AddAsync(account);
            await _cacheService.RemoveByPatternAsync($"CurrentAccounts_Company_{dto.CompanyId}*");

            return _mapper.Map<CurrentAccountListDto>(account);
        }

        public async Task<CurrentAccountListDto> GetByIdAsync(int id)
        {
            var cacheKey = $"CurrentAccount_Single_{id}";
            var cachedData = await _cacheService.GetAsync<CurrentAccountListDto>(cacheKey);
            if (cachedData != null) return cachedData;

            var account = await _currentAccountRepository.GetByIdAsync(id);
            if (account == null) throw new BusinessException(ErrorKeys.CurrentAccountNotFound);

            var mappedData = _mapper.Map<CurrentAccountListDto>(account);
            await _cacheService.SetAsync(cacheKey, mappedData, 60);

            return mappedData;
        }

        public async Task<PagedResponse<CurrentAccountListDto>> GetAllCurrentAccountsAsync(CurrentAccountFilterDto filter, int companyId)
        {
            var cacheKey = $"CurrentAccounts_Company_{companyId}_Page_{filter.PageNumber}_Size_{filter.PageSize}_Name_{filter.Name}_Type_{filter.Type}_Balance_{filter.balance}";
            var cachedData = await _cacheService.GetAsync<PagedResponse<CurrentAccountListDto>>(cacheKey);
            if (cachedData != null) return cachedData;

            var validFilter = new CurrentAccountFilterDto(filter.PageNumber, filter.PageSize);
            var query = _currentAccountRepository.GetQueryable()
                                                 .AsNoTracking()
                                                 .Where(x => x.CompanyId == companyId);

            if (!string.IsNullOrWhiteSpace(filter.Name))
                query = query.Where(x => x.Name.Contains(filter.Name));
            if (filter.Type.HasValue)
                query = query.Where(x => x.Type == filter.Type.Value);
            if (filter.balance.HasValue)
                query = query.Where(x => x.Balance <= filter.balance.Value);

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

        public async Task<CurrentAccountListDto> UpdateAsync(UpdateCurrentAccountDto dto)
        {
            await ValidateForUpdateAsync(dto);

            var account = await _currentAccountRepository.GetByIdAsync(dto.Id);
            if (account == null) throw new BusinessException(ErrorKeys.CurrentAccountNotFound);

            _mapper.Map(dto, account);
            _currentAccountRepository.Update(account);

            await _cacheService.RemoveAsync($"CurrentAccount_Single_{dto.Id}");
            await _cacheService.RemoveByPatternAsync($"CurrentAccounts_Company_{account.CompanyId}*");

            return _mapper.Map<CurrentAccountListDto>(account);
        }

        public async Task DeleteAsync(int id)
        {
            var account = await _currentAccountRepository.GetByIdAsync(id);
            if (account == null) throw new BusinessException(ErrorKeys.CurrentAccountNotFound);

            _currentAccountRepository.Delete(account);
            await _cacheService.RemoveAsync($"CurrentAccount_Single_{id}");
            await _cacheService.RemoveByPatternAsync($"CurrentAccounts_Company_{account.CompanyId}*");
        }
    }
}