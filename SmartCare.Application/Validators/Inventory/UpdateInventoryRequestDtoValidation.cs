using FluentValidation;
using SmartCare.Application.DTOs.Inventory.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Validators.Inventory
{
    public class UpdateInventoryRequestDtoValidation : AbstractValidator<UpdateInventoryRequestDto>
    {
        public UpdateInventoryRequestDtoValidation()
        {
            RuleFor(x => x.InventoryId)
                .NotEmpty().WithMessage("Inventory Id is required");
            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("StockQuantity cannot be negative.");

            RuleFor(x => x.ReservedQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("ReservedQuantity cannot be negative.");
        }
    }
}
