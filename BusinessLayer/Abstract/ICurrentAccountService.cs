using EntityLayer.DTOs.CurrentAccount;
using EntityLayer.DTOs.Pagination;
using EntityLayer.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Abstract
{
    public interface ICurrentAccountService
    {
        Task<PagedResponse<CurrentAccountListDto>> GetAllCurrentAccountsAsync(CurrentAccountFilterDto filter, int companyId);
        Task<CurrentAccountListDto> GetByIdAsync(int id);
        Task AddAsync(CreateCurrentAccountDto dto);
        Task UpdateAsync(UpdateCurrentAccountDto dto);
        Task DeleteAsync(int id);
        

    }
}