using BusinessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.Invoice;
using EntityLayer.DTOs.Pagination;
using EntityLayer.Entities.Domain;
using EntityLayer.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.NetworkInformation;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ApiLayer.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : BaseController
    {
        private readonly IInvoiceService _invoiceService;
        private readonly ICompanyService _companyService;
        private readonly ILogger<InvoicesController> _logger;

        public InvoicesController(IInvoiceService invoiceService, ICompanyService companyService, ILogger<InvoicesController> logger)
        {
            _invoiceService = invoiceService;
            _companyService = companyService;
            _logger = logger;
        }

        private async Task<bool> HasAccessToCompany(int companyId)
        {
            if (User.IsInRole("Admin")) return true;
            var company = await _companyService.GetByIdAsync(companyId);
            int loggedInUserId = GetUserId();
            return company != null && company.UserId == loggedInUserId;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] InvoiceFilterDto filter, [FromQuery] int companyId)
        {
            if (!await HasAccessToCompany(companyId))
                throw new BusinessException(ErrorKeys.Unauthorized);

            var result = await _invoiceService.GetAllInvoicesAsync(filter, companyId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var invoice = await _invoiceService.GetByIdAsync(id);
            if (!await HasAccessToCompany(invoice.CompanyId))
                throw new BusinessException(ErrorKeys.Unauthorized);

            return Ok(invoice);
        }

        [HttpPost("create-draft")]
        public async Task<IActionResult> CreateDraft([FromBody] CreateInvoiceDto dto)
        {
            if (!await HasAccessToCompany(dto.CompanyId))
                throw new BusinessException(ErrorKeys.Unauthorized);

            _logger.LogInformation("Creating draft invoice for CompanyId: {CompanyId}, Serial: {SerialNumber}", dto.CompanyId, dto.SerialNumber);
            await _invoiceService.CreateDraftInvoiceAsync(dto);
            return Ok(new { Message = "Draft invoice created successfully." });
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var invoice = await _invoiceService.GetByIdAsync(id);
            if (!await HasAccessToCompany(invoice.CompanyId))
                throw new BusinessException(ErrorKeys.Unauthorized);

            _logger.LogInformation("Invoice approval initiated. InvoiceId: {InvoiceId}", id);
            await _invoiceService.ApproveInvoiceAsync(id);
            return Ok(new { Message = "Invoice approved successfully. Stock and account balances updated." });
        }
        [HttpPost("{id}/send")]
        public async Task<IActionResult> Send(int id)
        {
            await _invoiceService.SendInvoiceToIntegratorAsync(id);

            return Ok(new
            {
                success = true,
                message = "The invoice was successfully transmitted to the official authorities / integrator and its status was updated to ‘Sent’"

            });
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDraft(int id)
        {
            var invoice = await _invoiceService.GetByIdAsync(id);
            if (!await HasAccessToCompany(invoice.CompanyId))
                throw new BusinessException(ErrorKeys.Unauthorized);

            _logger.LogWarning("Invoice deletion triggered. InvoiceId: {InvoiceId}", id);
            await _invoiceService.DeleteDraftInvoiceAsync(id);
            return Ok(new { Message = "Draft invoice deleted successfully." });
        }
    }
}