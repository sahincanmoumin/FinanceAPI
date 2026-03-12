using AutoMapper;
using BusinessLayer.Abstract;
using DataAccessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.Company;
using EntityLayer.DTOs.Pagination;
using EntityLayer.Entities.Auth;
using EntityLayer.Entities.Domain;
using EntityLayer.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLayer.Concrete
{
    public class CompanyService : ICompanyService
    {
        private readonly IGenericRepository<Company> _companyRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateCompanyDto> _createValidator;
        private readonly IValidator<UpdateCompanyDto> _updateValidator;

        public CompanyService(
            IGenericRepository<Company> companyRepository,
            IMapper mapper,
            IValidator<CreateCompanyDto> createValidator,
            IValidator<UpdateCompanyDto> updateValidator)
        {
            _companyRepository = companyRepository;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        // --- PRIVATE VALIDATION ---
        private async Task ValidateForCreateAsync(CreateCompanyDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new BusinessException(errors);
            }

            var isExist = await _companyRepository.AnyAsync(x => x.TaxNumber == dto.TaxNumber);
            if (isExist) throw new BusinessException(ErrorKeys.CompanyAlreadyExists);
        }

        private async Task ValidateForUpdateAsync(UpdateCompanyDto dto)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new BusinessException(errors);
            }

            var isExist = await _companyRepository.AnyAsync(x => x.TaxNumber == dto.TaxNumber && x.Id != dto.Id);
            if (isExist) throw new BusinessException(ErrorKeys.CompanyAlreadyExists);
        }

        // --- ANA METOTLAR ---
        public async Task<PagedResponse<CompanyListDto>> GetAllCompaniesAsync(CompanyFilterDto filter, int userId)
        {
            var validFilter = new CompanyFilterDto(filter.PageNumber, filter.PageSize);
            var query = _companyRepository.GetQueryable()
                                          .AsNoTracking()
                                          .Where(c => c.UserId == userId);

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                query = query.Where(c => c.Name.Contains(filter.Name));
            }

            if (!string.IsNullOrWhiteSpace(filter.Address))
            {
                query = query.Where(c => c.Address.Contains(filter.Address));
            }

            var totalRecords = await query.CountAsync();

            var companies = await query
                .OrderByDescending(c => c.CreateDate)
                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                .Take(validFilter.PageSize)
                .ToListAsync();

            var mappedCompanies = _mapper.Map<IEnumerable<CompanyListDto>>(companies);

            return new PagedResponse<CompanyListDto>(mappedCompanies, totalRecords, validFilter.PageNumber, validFilter.PageSize);
        }

        public async Task<CompanyListDto> GetByIdAsync(int id)
        {
            var company = await _companyRepository.GetByIdAsync(id);
            if (company == null) throw new BusinessException(ErrorKeys.CompanyNotFound);
            return _mapper.Map<CompanyListDto>(company);
        }

        public async Task AddAsync(CreateCompanyDto dto)
        {
            await ValidateForCreateAsync(dto);

            var company = _mapper.Map<Company>(dto);
            await _companyRepository.AddAsync(company);
        }

        public async Task UpdateAsync(UpdateCompanyDto dto)
        {
            await ValidateForUpdateAsync(dto);

            var company = await _companyRepository.GetByIdAsync(dto.Id);
            if (company == null) throw new BusinessException(ErrorKeys.CompanyNotFound);

            _mapper.Map(dto, company);
            _companyRepository.Update(company);
        }

        public async Task DeleteAsync(int id)
        {
            var company = await _companyRepository.GetByIdAsync(id);
            if (company == null) throw new BusinessException(ErrorKeys.CompanyNotFound);

            _companyRepository.Delete(company);
        }
    }
}