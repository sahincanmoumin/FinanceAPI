using EntityLayer.DTOs.Pagination;
using EntityLayer.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class StockFilterDto : PaginationFilter
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public UnitType? Unit { get; set; }
    public decimal? MinBalance { get; set; }

    public StockFilterDto(){}
    public StockFilterDto(int pageNumber, int pageSize) : base(pageNumber, pageSize)
    {
    }
}