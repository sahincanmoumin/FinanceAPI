using EntityLayer.DTOs.Invoice;
using FluentValidation;
using System;

namespace BusinessLayer.ValidationRules.Invoice
{
    public class CreateInvoiceValidator : AbstractValidator<CreateInvoiceDto>
    {
        public CreateInvoiceValidator()
        {
            RuleFor(x => x.SerialNumber)
                .NotEmpty().WithMessage("Invoice number cannot be empty.")
                .MaximumLength(16).WithMessage("Invoice number cannot exceed 16 characters.");

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Invoice date cannot be empty.")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Invoice date cannot be in the future.");

            RuleFor(x => x.CurrentAccountId)
                .GreaterThan(0).WithMessage("A valid current account must be selected.");

            RuleFor(x => x.CompanyId)
                .GreaterThan(0).WithMessage("A valid company must be selected.");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Invalid invoice type selected.");
        }
    }
}