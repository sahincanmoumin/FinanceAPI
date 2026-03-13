using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityLayer.DTOs.Invoice;
using EntityLayer.DTOs.Pagination;
using System.Threading.Tasks;

namespace BusinessLayer.Abstract
{
    public interface IInvoiceService
    {
        Task<PagedResponse<InvoiceListDto>> GetAllInvoicesAsync(InvoiceFilterDto filter, int companyId);
        Task<InvoiceListDto> GetByIdAsync(int id);
        Task<InvoiceListDto> CreateDraftInvoiceAsync(CreateInvoiceDto dto);
        Task ApproveInvoiceAsync(int invoiceId);
        Task DeleteDraftInvoiceAsync(int invoiceId);
        public Task SendInvoiceToIntegratorAsync(int invoiceId);

    }
}
