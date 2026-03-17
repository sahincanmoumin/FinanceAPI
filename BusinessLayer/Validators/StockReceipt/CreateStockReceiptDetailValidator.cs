using EntityLayer.DTOs.StockReceipt;
using EntityLayer.DTOs.StockReceiptDetail;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.ValidationRules.StockReceipt
{
    public class CreateStockReceiptDetailValidator : AbstractValidator<CreateStockReceiptDetailDto>
    {
        public CreateStockReceiptDetailValidator()
        {
            RuleFor(x => x.StockId)
                .GreaterThan(0).WithMessage("A valid stock must be selected.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative.");
        }
    }
}