using EntityLayer.DTOs.StockReceipt;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Validators.StockReceipt
{
    public class UpdateStockReceiptValidator : AbstractValidator<UpdateStockReceiptDto>
    {
        public UpdateStockReceiptValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Invalid receipt ID.");

            RuleFor(x => x.SerialNumber)
                .NotEmpty().WithMessage("Serial number cannot be empty.")
                .MaximumLength(16).WithMessage("Serial number cannot exceed 16 characters.");

            RuleFor(x => x.WarehouseId)
                .GreaterThan(0).WithMessage("A valid warehouse must be selected.");

            RuleFor(x => x.Details)
                .NotEmpty().WithMessage("Receipt must contain at least one detail line.");

            RuleForEach(x => x.Details).SetValidator(new UpdateStockReceiptDetailValidator());
        }
    }
}
