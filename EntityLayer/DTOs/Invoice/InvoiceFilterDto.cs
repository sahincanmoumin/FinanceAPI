using EntityLayer.DTOs.Pagination;
using EntityLayer.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class InvoiceFilterDto : PaginationFilter
{
    public string? SerialNumber { get; set; }
    public InvoiceStatus? Status { get; set; }
    public InvoiceType? Type { get; set; } 
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // detaillll
    public int? StockId { get; set; }
    public decimal? MinUnitPrice { get; set; }

    public InvoiceFilterDto()
    {
    }
    public InvoiceFilterDto(int pageNumber, int pageSize) : base(pageNumber, pageSize)
    {
    }
}