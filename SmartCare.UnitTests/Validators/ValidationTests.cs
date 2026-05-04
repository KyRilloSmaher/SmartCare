using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using SmartCare.Application.Companies.Requests;
using SmartCare.Application.DTOs.Auth.Requests;
using SmartCare.Application.Validators.Auth;
using SmartCare.Application.Validators.Companies;

namespace SmartCare.UnitTests.Validators;

public class ValidationTests
{
    [Fact]
    public void LoginValidator_ShouldFail_WhenEmailIsInvalid()
    {
        var validator = new LoginRequestDtoValidator();
        var dto = new LoginRequestDto { Email = "bad-email", Password = "pass" };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void LoginValidator_ShouldPass_WhenPayloadIsValid()
    {
        var validator = new LoginRequestDtoValidator();
        var dto = new LoginRequestDto { Email = "user@example.com", Password = "secret" };

        var result = validator.TestValidate(dto);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateCompanyValidator_ShouldFail_WhenLogoMissing()
    {
        var validator = new CreateCompanyRequestDtoValidator();
        var dto = new CreateCompanyRequestDto { Name = "Pfizer", Logo = null! };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Logo);
    }

    [Fact]
    public void CreateCompanyValidator_ShouldFail_WhenLogoExtensionInvalid()
    {
        var validator = new CreateCompanyRequestDtoValidator();
        var file = new FormFile(new MemoryStream([1, 2, 3]), 0, 3, "logo", "logo.gif");
        var dto = new CreateCompanyRequestDto { Name = "Pfizer", Logo = file };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Logo);
    }
}
