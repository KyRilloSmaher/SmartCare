using FluentValidation;
using SmartCare.Application.DTOs.Orders.Requests;

namespace SmartCare.Application.Validators.Orders
{
    public class CreateOrderRequestDtoValidator : AbstractValidator<CreateOrderRequestDto>
    {
        public CreateOrderRequestDtoValidator()
        {
            RuleFor(x => x.clientId)
                .NotEmpty().WithMessage("ClientId is required.");


            RuleFor(x => x.cartId)
                .NotEmpty().WithMessage("CartId is required.")
                .NotEqual(Guid.Empty).WithMessage("CartId cannot be an empty GUID.");

            RuleFor(x => x.orderType)
                .IsInEnum().WithMessage("Invalid order type.");

            // StoreId — only required if the order type requires store pickup (example: InStore)
            When(x => x.orderType == Domain.Enums.OrderType.InStore, () =>
            {
                RuleFor(x => x.storeId)
                    .NotNull().WithMessage("StoreId is required for in-store orders.")
                    .NotEqual(Guid.Empty).WithMessage("StoreId cannot be empty.");
            });

            When(x => x.orderType == Domain.Enums.OrderType.Online, () =>
            {
                RuleFor(x => x.deliveryAddress)
                    .NotEmpty().WithMessage("Delivery address is required for delivery orders.");
            });
        }
    }
}
