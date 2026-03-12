using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityLayer.DTOs.Company;
using FluentValidation;

namespace BusinessLayer.ValidationRules.Company
{
    public class UpdateCompanyValidator : AbstractValidator<UpdateCompanyDto>
    {
        public UpdateCompanyValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Invalid company ID.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Company name cannot be empty.")
                .MinimumLength(2).WithMessage("Company name must be at least 2 characters.");

            RuleFor(x => x.TaxOffice)
                .NotEmpty().WithMessage("Tax office cannot be empty.");

            RuleFor(x => x.TaxNumber)
                .NotEmpty().WithMessage("Tax number cannot be empty.")
                .Length(10, 11).WithMessage("Tax number must be 10 or 11 characters long.");
            RuleFor(x=> x.Address)
                .NotEmpty().WithMessage("Address cannot be empty.")
                .MinimumLength(2).WithMessage("Address must be at least 2 characters long.");
        }
    }
}