using EntityLayer.DTOs.Pagination;
using EntityLayer.Entities.Enums;

public class StockTransFilterDto : PaginationFilter
{
    public TransactionType? Direction { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public decimal? MinQuantity { get; set; }
    public decimal? MaxQuantity { get; set; }
    public decimal? MinUnitPrice { get; set; }
    public decimal? MaxUnitPrice { get; set; }

    public StockTransFilterDto(){

    }
    public StockTransFilterDto(int pageNumber, int pageSize) : base(pageNumber, pageSize)
    {
    }
}