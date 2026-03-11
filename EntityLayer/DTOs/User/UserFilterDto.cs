using EntityLayer.DTOs.Pagination;

public class UserFilterDto : PaginationFilter
{
    public string? Name { get; set; }

    public UserFilterDto() { }
    public UserFilterDto(int pageNumber, int pageSize) : base(pageNumber, pageSize)
    {
    }
}