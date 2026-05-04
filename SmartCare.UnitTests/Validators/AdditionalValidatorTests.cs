using FluentValidation.TestHelper;
using SmartCare.Application.DTOs.Address.Requests;
using SmartCare.Application.DTOs.Favorites.Requests;
using SmartCare.Application.DTOs.Rates.Requests;
using SmartCare.Application.DTOs.Stores.Requests;
using SmartCare.Application.Validators.Address;
using SmartCare.Application.Validators.Favourite;
using SmartCare.Application.Validators.Rates;
using SmartCare.Application.Validators.Store;

namespace SmartCare.UnitTests.Validators;

public class AdditionalValidatorTests
{
    #region CreateAddressValidator

    [Fact]
    public void CreateAddress_ShouldFail_WhenAddressIsEmpty()
    {
        var validator = new CreateAddressRequestDtoValidator();
        var dto = new CreateAddressRequestDto { address = "", Label = "Home", Latitude = 30.0f, Longitude = 31.0f };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.address);
    }

    [Fact]
    public void CreateAddress_ShouldFail_WhenLabelIsEmpty()
    {
        var validator = new CreateAddressRequestDtoValidator();
        var dto = new CreateAddressRequestDto { address = "123 Main St", Label = "", Latitude = 30.0f, Longitude = 31.0f };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Label);
    }

    [Fact]
    public void CreateAddress_ShouldFail_WhenLatitudeIsOutOfRange()
    {
        var validator = new CreateAddressRequestDtoValidator();
        var dto = new CreateAddressRequestDto { address = "123 Main St", Label = "Home", Latitude = 100, Longitude = 31.0f };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Latitude);
    }

    [Fact]
    public void CreateAddress_ShouldFail_WhenLongitudeIsOutOfRange()
    {
        var validator = new CreateAddressRequestDtoValidator();
        var dto = new CreateAddressRequestDto { address = "123 Main St", Label = "Home", Latitude = 30.0f, Longitude = 200 };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Longitude);
    }

    [Fact]
    public void CreateAddress_ShouldPass_WhenPayloadIsValid()
    {
        var validator = new CreateAddressRequestDtoValidator();
        var dto = new CreateAddressRequestDto { address = "123 Main St", Label = "Home", Latitude = 30.0f, Longitude = 31.0f };

        var result = validator.TestValidate(dto);
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region CreateStoreValidator

    [Fact]
    public void CreateStore_ShouldFail_WhenNameIsEmpty()
    {
        var validator = new CreateStoreRequestDtoValidator();
        var dto = new CreateStoreRequestDto { Name = "", Address = "addr", Latitude = 30, Longitude = 31, Phone = "+201234567890" };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateStore_ShouldFail_WhenLatitudeOutOfRange()
    {
        var validator = new CreateStoreRequestDtoValidator();
        var dto = new CreateStoreRequestDto { Name = "Store", Address = "addr", Latitude = 100, Longitude = 31, Phone = "+201234567890" };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Latitude);
    }

    [Fact]
    public void CreateStore_ShouldFail_WhenAddressIsEmpty()
    {
        var validator = new CreateStoreRequestDtoValidator();
        var dto = new CreateStoreRequestDto { Name = "Store", Address = "", Latitude = 30, Longitude = 31, Phone = "+201234567890" };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Address);
    }

    #endregion

    #region CreateRateValidator

    [Fact]
    public void CreateRate_ShouldFail_WhenProductIdIsEmpty()
    {
        var validator = new CreateRateRequestDtoValidation();
        var dto = new CreateRateRequestDto { ProductId = Guid.Empty, Value = 3 };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ProductId);
    }

    [Fact]
    public void CreateRate_ShouldFail_WhenValueIsZero()
    {
        var validator = new CreateRateRequestDtoValidation();
        var dto = new CreateRateRequestDto { ProductId = Guid.NewGuid(), Value = 0 };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Value);
    }

    [Fact]
    public void CreateRate_ShouldFail_WhenValueExceedsFive()
    {
        var validator = new CreateRateRequestDtoValidation();
        var dto = new CreateRateRequestDto { ProductId = Guid.NewGuid(), Value = 6 };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Value);
    }

    [Fact]
    public void CreateRate_ShouldPass_WhenPayloadIsValid()
    {
        var validator = new CreateRateRequestDtoValidation();
        var dto = new CreateRateRequestDto { ProductId = Guid.NewGuid(), Value = 4 };

        var result = validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.ProductId);
        result.ShouldNotHaveValidationErrorFor(x => x.Value);
    }

    #endregion

    #region CreateFavouriteValidator

    [Fact]
    public void CreateFavourite_ShouldFail_WhenProductIdIsEmpty()
    {
        var validator = new CreateFavouriteRequestDtoValidation();
        var dto = new CreateFavouriteRequestDto { ProductId = Guid.Empty, ClientId = "c1" };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ProductId);
    }

    [Fact]
    public void CreateFavourite_ShouldFail_WhenClientIdIsEmpty()
    {
        var validator = new CreateFavouriteRequestDtoValidation();
        var dto = new CreateFavouriteRequestDto { ProductId = Guid.NewGuid(), ClientId = "" };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ClientId);
    }

    [Fact]
    public void CreateFavourite_ShouldPass_WhenPayloadIsValid()
    {
        var validator = new CreateFavouriteRequestDtoValidation();
        var dto = new CreateFavouriteRequestDto { ProductId = Guid.NewGuid(), ClientId = "c1" };

        var result = validator.TestValidate(dto);
        result.IsValid.Should().BeTrue();
    }

    #endregion
}
