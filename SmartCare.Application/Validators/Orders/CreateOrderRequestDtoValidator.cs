using FluentValidation;
using SmartCare.Application.DTOs.Orders.Requests;

namespace SmartCare.Application.Validators.Orders
{
    public class CreateOrderRequestDtoValidator : AbstractValidator<CreateOrderRequestDto>
    {
        public CreateOrderRequestDtoValidator()
        {


            RuleFor(x => x.CartId)
                .NotEmpty().WithMessage("CartId is required.")
                .NotEqual(Guid.Empty).WithMessage("CartId cannot be an empty GUID.");


        }
    }
}
