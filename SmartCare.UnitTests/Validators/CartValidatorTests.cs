using FluentValidation.TestHelper;
using SmartCare.Application.DTOs.Cart.Requests;
using SmartCare.Application.Validators.Carts;

namespace SmartCare.UnitTests.Validators;

public class CartValidatorTests
{
    #region AddToCartValidator

    [Fact]
    public void AddToCart_ShouldFail_WhenCartIdIsEmpty()
    {
        var validator = new AddToCartRequestValidator();
        var dto = new AddToCartRequestDto { CartId = Guid.Empty, ProductId = Guid.NewGuid(), Quantity = 1 };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.CartId);
    }

    [Fact]
    public void AddToCart_ShouldFail_WhenProductIdIsEmpty()
    {
        var validator = new AddToCartRequestValidator();
        var dto = new AddToCartRequestDto { CartId = Guid.NewGuid(), ProductId = Guid.Empty, Quantity = 1 };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ProductId);
    }

    [Fact]
    public void AddToCart_ShouldFail_WhenQuantityIsZero()
    {
        var validator = new AddToCartRequestValidator();
        var dto = new AddToCartRequestDto { CartId = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 0 };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void AddToCart_ShouldFail_WhenQuantityExceeds100()
    {
        var validator = new AddToCartRequestValidator();
        var dto = new AddToCartRequestDto { CartId = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 101 };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void AddToCart_ShouldPass_WhenPayloadIsValid()
    {
        var validator = new AddToCartRequestValidator();
        var dto = new AddToCartRequestDto { CartId = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 3 };

        var result = validator.TestValidate(dto);
        result.IsValid.Should().BeTrue();
    }

    #endregion
}
