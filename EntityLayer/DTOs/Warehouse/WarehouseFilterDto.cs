using EntityLayer.DTOs.Pagination;
using EntityLayer.Entities.Enums;

namespace EntityLayer.DTOs.Warehouse
{
    public class WarehouseFilterDto : PaginationFilter
    {
        public WarehouseFilterDto() : base()
        {
        }

        public WarehouseFilterDto(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public int? CompanyId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Address { get; set; }
        public WarehouseType? Type { get; set; }

    }
}