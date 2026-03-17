using EntityLayer.DTOs.StockReceipt;
using EntityLayer.DTOs.StockReceiptDetail;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityLayer.DTOs.StockReceipt;
using FluentValidation;
using System;

namespace BusinessLayer.ValidationRules.StockReceipt
{
    public class CreateStockReceiptValidator : AbstractValidator<CreateStockReceiptDto>
    {
        public CreateStockReceiptValidator()
        {
            RuleFor(x => x.SerialNumber)
                .NotEmpty().WithMessage("Serial number cannot be empty.")
                .MaximumLength(16).WithMessage("Serial number cannot exceed 16 characters.");

            RuleFor(x => x.CompanyId)
                .GreaterThan(0).WithMessage("A valid company must be selected.");

            RuleFor(x => x.WarehouseId)
                .GreaterThan(0).WithMessage("A valid warehouse must be selected.");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Invalid receipt type selected.");

            RuleFor(x => x.Details)
                .NotEmpty().WithMessage("Receipt must contain at least one detail line.");

            RuleForEach(x => x.Details).SetValidator(new CreateStockReceiptDetailValidator());
        }
    }
}