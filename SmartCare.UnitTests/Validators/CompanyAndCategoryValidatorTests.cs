using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using SmartCare.Application.Companies.Requests;
using SmartCare.Application.DTOs.Caregory.Requests;
using SmartCare.Application.Validators.Companies;

namespace SmartCare.UnitTests.Validators;

public class CompanyAndCategoryValidatorTests
{
    #region CreateCompanyValidator

    [Fact]
    public void CreateCompany_ShouldFail_WhenLogoMissing()
    {
        var validator = new CreateCompanyRequestDtoValidator();
        var dto = new CreateCompanyRequestDto { Name = "Pfizer", Logo = null! };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Logo);
    }

    [Fact]
    public void CreateCompany_ShouldFail_WhenLogoExtensionInvalid()
    {
        var validator = new CreateCompanyRequestDtoValidator();
        var file = new FormFile(new MemoryStream(new byte[] { 1, 2, 3 }), 0, 3, "logo", "logo.gif");
        var dto = new CreateCompanyRequestDto { Name = "Pfizer", Logo = file };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Logo);
    }

    [Fact]
    public void CreateCompany_ShouldFail_WhenNameIsEmpty()
    {
        var validator = new CreateCompanyRequestDtoValidator();
        var file = new FormFile(new MemoryStream(new byte[] { 1, 2, 3 }), 0, 3, "logo", "logo.png");
        var dto = new CreateCompanyRequestDto { Name = "", Logo = file };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    #endregion

    #region CreateCategoryValidator

    [Fact]
    public void CreateCategory_ShouldFail_WhenNameIsEmpty()
    {
        var validator = new SmartCare.Application.Validators.Category.CreateCompanyRequestDtoValidator();
        var file = new FormFile(new MemoryStream(new byte[] { 1, 2, 3 }), 0, 3, "logo", "logo.png");
        var dto = new CreateCategoryRequestDto { Name = "", Logo = file };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateCategory_ShouldFail_WhenLogoIsNull()
    {
        var validator = new SmartCare.Application.Validators.Category.CreateCompanyRequestDtoValidator();
        var dto = new CreateCategoryRequestDto { Name = "Pain", Logo = null! };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Logo);
    }

    [Fact]
    public void CreateCategory_ShouldFail_WhenLogoExtensionInvalid()
    {
        var validator = new SmartCare.Application.Validators.Category.CreateCompanyRequestDtoValidator();
        var file = new FormFile(new MemoryStream(new byte[] { 1, 2, 3 }), 0, 3, "logo", "logo.bmp");
        var dto = new CreateCategoryRequestDto { Name = "Pain", Logo = file };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Logo);
    }

    #endregion
}
