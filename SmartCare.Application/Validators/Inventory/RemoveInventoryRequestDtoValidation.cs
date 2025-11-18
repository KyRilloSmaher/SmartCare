using FluentValidation;
using SmartCare.Application.DTOs.Inventory.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Validators.Inventory
{
    public class RemoveInventoryRequestDtoValidation : AbstractValidator<RemoveInventoryRequestDto>
    {
        public RemoveInventoryRequestDtoValidation()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("InventoryId is required.");
        }
    }
}
