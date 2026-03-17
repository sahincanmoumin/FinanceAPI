using EntityLayer.DTOs.Warehouse;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Validators.WarehouseValidator
{
    public class UpdateWarehouseValidator : AbstractValidator<UpdateWarehouseDto>
    {
        public UpdateWarehouseValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Invalid warehouse ID.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Warehouse code cannot be empty.")
                .MaximumLength(16).WithMessage("Warehouse code cannot exceed 16 characters.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Warehouse name cannot be empty.")
                .MaximumLength(100).WithMessage("Warehouse name cannot exceed 100 characters.");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Invalid warehouse type selected.");

            RuleFor(x => x.Address)
                .MaximumLength(500).WithMessage("Address cannot exceed 500 characters.");
        }
    }
}
