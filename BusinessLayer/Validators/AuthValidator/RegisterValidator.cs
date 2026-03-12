using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityLayer.DTOs.Auth;
using FluentValidation;

namespace BusinessLayer.ValidationRules.Auth
{
    public class RegisterValidator : AbstractValidator<RegisterDto>
    {
        public RegisterValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("User name cannot be empty.")
                .MinimumLength(2).WithMessage("First name must be at least 2 characters.");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Fullname cannot be empty.")
                .MinimumLength(2).WithMessage("Last name must be at least 2 characters.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password cannot be empty.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");
        }
    }
}