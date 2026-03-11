using EntityLayer.DTOs.Pagination;
using EntityLayer.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CurrentAccountFilterDto : PaginationFilter
{
    public string? Name { get; set; }
    public AccountType? Type { get; set; }
    public decimal? balance { get; set; }
    public CurrentAccountFilterDto()
    {
    }
    public CurrentAccountFilterDto(int pageNumber, int pageSize) : base(pageNumber, pageSize)
    {
    }
}