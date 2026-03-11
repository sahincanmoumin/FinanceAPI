using EntityLayer.DTOs.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CompanyFilterDto : PaginationFilter
{
    public string? Name { get; set; }
    public string? Address { get; set; }

    public CompanyFilterDto() { }
    public CompanyFilterDto(int pageNumber, int pageSize) : base(pageNumber, pageSize)
    {
    }
}