using EntityLayer.DTOs.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class UserRoleFilterDto : PaginationFilter
{
    public string? RoleName { get; set; }
    public UserRoleFilterDto() { }
    public UserRoleFilterDto(int pageNumber, int pageSize) : base(pageNumber, pageSize)
    {
    }
}