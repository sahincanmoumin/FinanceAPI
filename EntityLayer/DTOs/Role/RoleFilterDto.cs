using EntityLayer.DTOs.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class RoleFilterDto : PaginationFilter
{
    public string? Name { get; set; }
    public RoleFilterDto() {
    }
    public RoleFilterDto(int pageNumber, int pageSize) : base(pageNumber, pageSize)
    {
    }
}