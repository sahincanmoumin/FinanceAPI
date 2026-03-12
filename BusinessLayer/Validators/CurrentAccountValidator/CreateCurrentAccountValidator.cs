using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityLayer.DTOs.CurrentAccount;
using EntityLayer.Entities.Enums;
using FluentValidation;

namespace BusinessLayer.Validators.CurrentAccountValidator
{
    public class CreateCurrentAccountValidator : AbstractValidator<CreateCurrentAccountDto>
    {
        public CreateCurrentAccountValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Current account code cannot be empty.")
                .MaximumLength(50).WithMessage("Current account code cannot exceed 50 characters.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Current account name cannot be empty.")
                .MinimumLength(2).WithMessage("Current account name must be at least 2 characters.");

            RuleFor(x => x.CompanyId)
                .GreaterThan(0).WithMessage("A valid company must be selected.");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Invalid account type selected.");
        }
    }
}