using FluentValidation;
using SmartCare.Application.DTOs.Inventory.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Validators.Inventory
{
    public class CreateInventoryRequestDtoValidation : AbstractValidator<CreateInventoryRequestDto>
    {
        public CreateInventoryRequestDtoValidation()
        {
            RuleFor(x => x.StoreId)
                .NotEmpty().WithMessage("StoreId is required.");

            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("ProductId is required.");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("StockQuantity cannot be negative.");

            RuleFor(x => x.ReservedQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("ReservedQuantity cannot be negative.");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(x => x.ReservedQuantity).WithMessage("ReservedQuantity cannot be Greater Than StockQuantity.");
        }
    }
}
