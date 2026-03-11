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

        public CurrentAccountService(ICurrentAccountRepository currentAccountRepository, IMapper mapper)
        {
            _currentAccountRepository = currentAccountRepository;
            _mapper = mapper;
        }

        public async Task<CurrentAccountListDto> GetByIdAsync(int id)
        {
            var account = await _currentAccountRepository.GetByIdAsync(id);
            if (account == null) throw new BusinessException(ErrorKeys.CurrentAccountNotFound);
            return _mapper.Map<CurrentAccountListDto>(account);
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
        }

        public async Task<PagedResponse<CurrentAccountListDto>> GetAllCurrentAccountsAsync(CurrentAccountFilterDto filter, int companyId)
        {
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

            return new PagedResponse<CurrentAccountListDto>(mappedAccounts, totalRecords, validFilter.PageNumber, validFilter.PageSize);
        }

        public async Task UpdateAsync(UpdateCurrentAccountDto dto)
        {
            var account = await _currentAccountRepository.GetByIdAsync(dto.Id);
            if (account == null) throw new BusinessException(ErrorKeys.CurrentAccountNotFound);

            _mapper.Map(dto, account);
            _currentAccountRepository.Update(account);
        }

        public async Task DeleteAsync(int id)
        {
            var account = await _currentAccountRepository.GetByIdAsync(id);
            if (account == null) throw new BusinessException(ErrorKeys.CurrentAccountNotFound);

            _currentAccountRepository.Delete(account);
        }
    }
}