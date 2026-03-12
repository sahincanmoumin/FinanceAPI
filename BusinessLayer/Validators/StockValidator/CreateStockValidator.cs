using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityLayer.DTOs.Stock;
using FluentValidation;

namespace BusinessLayer.Validators.StockValidator
{
    public class CreateStockValidator : AbstractValidator<CreateStockDto>
    {
        public CreateStockValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Stock code cannot be empty.")
                .MaximumLength(50).WithMessage("Stock code cannot exceed 50 characters.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Stock name cannot be empty.")
                .MinimumLength(2).WithMessage("Stock name must be at least 2 characters.");

            RuleFor(x => x.CompanyId)
                .GreaterThan(0).WithMessage("A valid company must be selected.");

            RuleFor(x => x.Unit)
                .IsInEnum().WithMessage("Invalid stock unit selected.");
        }
    }
}