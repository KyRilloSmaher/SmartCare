using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using SmartCare.Application.DTOs.Product.Requests;
using SmartCare.Application.Validators.Products;

namespace SmartCare.UnitTests.Validators;

public class ProductValidatorTests
{
    #region CreateProductValidator

    [Fact]
    public void CreateProduct_ShouldFail_WhenNameEnIsEmpty()
    {
        var validator = new CreateProductRequestDtoValidation();
        var dto = new CreateProductRequestDto
        {
            NameEn = "",
            Description = "desc",
            MedicalDescription = "med",
            Tags = "tag",
            ActiveIngredients = "ingredient",
            CategoryId = Guid.NewGuid(),
            CompanyId = Guid.NewGuid(),
            MainImage = CreateValidFormFile("test.png"),
            Images = new List<IFormFile> { CreateValidFormFile("img.png") }
        };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.NameEn);
    }

    [Fact]
    public void CreateProduct_ShouldFail_WhenDescriptionIsEmpty()
    {
        var validator = new CreateProductRequestDtoValidation();
        var dto = new CreateProductRequestDto
        {
            NameEn = "Product",
            Description = "",
            MedicalDescription = "med",
            Tags = "tag",
            ActiveIngredients = "ingredient",
            CategoryId = Guid.NewGuid(),
            CompanyId = Guid.NewGuid(),
            MainImage = CreateValidFormFile("test.png"),
            Images = new List<IFormFile> { CreateValidFormFile("img.png") }
        };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void CreateProduct_ShouldFail_WhenCategoryIdIsEmpty()
    {
        var validator = new CreateProductRequestDtoValidation();
        var dto = new CreateProductRequestDto
        {
            NameEn = "Product",
            Description = "desc",
            MedicalDescription = "med",
            Tags = "tag",
            ActiveIngredients = "ingredient",
            CategoryId = Guid.Empty,
            CompanyId = Guid.NewGuid(),
            MainImage = CreateValidFormFile("test.png"),
            Images = new List<IFormFile> { CreateValidFormFile("img.png") }
        };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
    }

    [Fact]
    public void CreateProduct_ShouldFail_WhenMainImageIsNull()
    {
        var validator = new CreateProductRequestDtoValidation();
        var dto = new CreateProductRequestDto
        {
            NameEn = "Product",
            Description = "desc",
            MedicalDescription = "med",
            Tags = "tag",
            ActiveIngredients = "ingredient",
            CategoryId = Guid.NewGuid(),
            CompanyId = Guid.NewGuid(),
            MainImage = null!,
            Images = new List<IFormFile> { CreateValidFormFile("img.png") }
        };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.MainImage);
    }

    [Fact]
    public void CreateProduct_ShouldFail_WhenDiscountOutOfRange()
    {
        var validator = new CreateProductRequestDtoValidation();
        var dto = new CreateProductRequestDto
        {
            NameEn = "Product",
            Description = "desc",
            MedicalDescription = "med",
            Tags = "tag",
            ActiveIngredients = "ingredient",
            CategoryId = Guid.NewGuid(),
            CompanyId = Guid.NewGuid(),
            DiscountPercentage = 150,
            MainImage = CreateValidFormFile("test.png"),
            Images = new List<IFormFile> { CreateValidFormFile("img.png") }
        };

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.DiscountPercentage);
    }

    #endregion

    private static IFormFile CreateValidFormFile(string fileName)
    {
        return new FormFile(new MemoryStream(new byte[] { 1, 2, 3 }), 0, 3, "file", fileName);
    }
}
