using EntityLayer.DTOs.Company;
using EntityLayer.DTOs.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BusinessLayer.Abstract
{
    public interface ICompanyService
    {
        Task<PagedResponse<CompanyListDto>> GetAllCompaniesAsync(CompanyFilterDto filter, int userId); 
        Task<CompanyListDto> GetByIdAsync(int id);
        Task AddAsync(CreateCompanyDto dto);
        Task UpdateAsync(UpdateCompanyDto dto);
        Task DeleteAsync(int id);
    }
}