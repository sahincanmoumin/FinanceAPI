using AutoMapper;
using BusinessLayer.Abstract;
using DataAccessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.Pagination;
using EntityLayer.DTOs.Warehouse;
using EntityLayer.Entities.Domain;
using EntityLayer.Entities.Enums;
using EntityLayer.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLayer.Concrete
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IGenericRepository<Warehouse> _warehouseRepository;
        private readonly IMapper _mapper;

        public WarehouseService(IGenericRepository<Warehouse> warehouseRepository, IMapper mapper)
        {
            _warehouseRepository = warehouseRepository;
            _mapper = mapper;
        }

        private async Task ValidateUniqueAsync(string code, int companyId)
        {
            var exists = await _warehouseRepository.AnyAsync(w => w.Code == code && w.CompanyId == companyId);
            if (exists)
                throw new BusinessException(ErrorKeys.WarehouseAlreadyExists);
        }

        private async Task ValidateSingleWRAsync(WarehouseType newType, int companyId)
        {
            if (newType == WarehouseType.Main)
            {
                var mainExists = await _warehouseRepository.AnyAsync(w => w.Type == WarehouseType.Main && w.CompanyId == companyId);
                if (mainExists)
                    throw new BusinessException(ErrorKeys.MainWarehouseAlreadyExists);
            }
        }

        public async Task<PagedResponse<WarehouseListDto>> GetAllAsync(WarehouseFilterDto filter)
        {
            var validFilter = new WarehouseFilterDto(filter.PageNumber, filter.PageSize);

            var query = _warehouseRepository.GetQueryable().AsNoTracking();

            if (filter.CompanyId.HasValue)
                query = query.Where(w => w.CompanyId == filter.CompanyId.Value);

            if (!string.IsNullOrWhiteSpace(filter.Name))
                query = query.Where(w => w.Name.Contains(filter.Name));

            if (!string.IsNullOrWhiteSpace(filter.Code))
                query = query.Where(w => w.Code.Contains(filter.Code));

            if (!string.IsNullOrWhiteSpace(filter.Address))
                query = query.Where(w => w.Address.Contains(filter.Address));

            if (filter.Type.HasValue)
                query = query.Where(w => w.Type == filter.Type.Value);

            var totalRecords = await query.CountAsync();

            var warehouses = await query
                .OrderByDescending(w => w.Id)
                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                .Take(validFilter.PageSize)
                .ToListAsync();

            var mappedData = _mapper.Map<IEnumerable<WarehouseListDto>>(warehouses);

            return new PagedResponse<WarehouseListDto>(mappedData, validFilter.PageNumber, validFilter.PageSize, totalRecords);
        }

        public async Task<WarehouseListDto> GetByIdAsync(int id)
        {
            var warehouse = await _warehouseRepository.GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == id);

            if (warehouse == null)
                throw new BusinessException(ErrorKeys.WarehouseNotFound);

            return _mapper.Map<WarehouseListDto>(warehouse);
        }

        public async Task<WarehouseListDto> AddAsync(CreateWarehouseDto dto)
        {
            await ValidateUniqueAsync(dto.Code, dto.CompanyId);
            await ValidateSingleWRAsync(dto.Type, dto.CompanyId);

            var warehouse = _mapper.Map<Warehouse>(dto);
            await _warehouseRepository.AddAsync(warehouse);

            return _mapper.Map<WarehouseListDto>(warehouse);
        }

        public async Task<WarehouseListDto> UpdateAsync(UpdateWarehouseDto dto)
        {
            var existingWarehouse = await _warehouseRepository.GetByIdAsync(dto.Id);
            if (existingWarehouse == null)
                throw new BusinessException(ErrorKeys.WarehouseNotFound);

            if (existingWarehouse.Code != dto.Code)
                await ValidateUniqueAsync(dto.Code, existingWarehouse.CompanyId);

            if (existingWarehouse.Type != dto.Type && dto.Type == WarehouseType.Main)
            {
                await ValidateSingleWRAsync(dto.Type, existingWarehouse.CompanyId);
            }

            _mapper.Map(dto, existingWarehouse);
            _warehouseRepository.Update(existingWarehouse);

            return _mapper.Map<WarehouseListDto>(existingWarehouse);
        }

        public async Task DeleteAsync(int id)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(id);
            if (warehouse == null)
                throw new BusinessException(ErrorKeys.WarehouseNotFound);

            _warehouseRepository.Delete(warehouse);
        }
    }
}