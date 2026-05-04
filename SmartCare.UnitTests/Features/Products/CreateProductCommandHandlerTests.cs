using CloudinaryDotNet.Actions;
using SmartCare.Application.DTOs.Product.Requests;
using SmartCare.Application.DTOs.Product.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Features.Product.Commands.Create;
using SmartCare.Application.IServices;
using SmartCare.Domain.Enums;

namespace SmartCare.UnitTests.Features.Products;

public class CreateProductCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenCategoryIsInvalid()
    {
        var categories = new Mock<ICategoryRepository>();
        categories.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), true)).ReturnsAsync((Category?)null);

        var companies = new Mock<ICompanyRepository>();
        companies.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), true)).ReturnsAsync(new Company());

        var uow = new UnitOfWorkMockBuilder()
            .WithCategories(categories.Object)
            .WithCompanies(companies.Object)
            .Build();

        var sut = new CreateProductCommandHandler(
            ResponseHandler, uow, Mock.Of<IImageUploaderService>(),
            Mock.Of<IRedisCacheService>(), Mock.Of<IMapper>(), Mock.Of<IBackgroundJobService>());

        var dto = new CreateProductRequestDto
        {
            NameEn = "Test", CategoryId = Guid.NewGuid(), CompanyId = Guid.NewGuid(),
            MainImage = Mock.Of<Microsoft.AspNetCore.Http.IFormFile>()
        };

        var result = await sut.Handle(new CreateProductCommand(dto), CT);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Contain("Invalid category or company");
    }

    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenCompanyIsInvalid()
    {
        var categories = new Mock<ICategoryRepository>();
        categories.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), true)).ReturnsAsync(new Category());

        var companies = new Mock<ICompanyRepository>();
        companies.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), true)).ReturnsAsync((Company?)null);

        var uow = new UnitOfWorkMockBuilder()
            .WithCategories(categories.Object)
            .WithCompanies(companies.Object)
            .Build();

        var sut = new CreateProductCommandHandler(
            ResponseHandler, uow, Mock.Of<IImageUploaderService>(),
            Mock.Of<IRedisCacheService>(), Mock.Of<IMapper>(), Mock.Of<IBackgroundJobService>());

        var dto = new CreateProductRequestDto
        {
            NameEn = "Test", CategoryId = Guid.NewGuid(), CompanyId = Guid.NewGuid(),
            MainImage = Mock.Of<Microsoft.AspNetCore.Http.IFormFile>()
        };

        var result = await sut.Handle(new CreateProductCommand(dto), CT);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Contain("Invalid category or company");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailed_WhenMainImageUploadFails()
    {
        var categories = new Mock<ICategoryRepository>();
        categories.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), true)).ReturnsAsync(new Category());

        var companies = new Mock<ICompanyRepository>();
        companies.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), true)).ReturnsAsync(new Company());

        var uploader = new Mock<IImageUploaderService>();
        uploader.Setup(x => x.UploadImageAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(), ImageFolder.ProductImages))
            .ReturnsAsync(new ImageUploadResult { Error = new Error { Message = "upload err" } });

        var uow = new UnitOfWorkMockBuilder()
            .WithCategories(categories.Object)
            .WithCompanies(companies.Object)
            .Build();

        var mapper = new Mock<IMapper>();
        mapper.Setup(x => x.Map<SmartCare.Domain.Entities.Product>(It.IsAny<CreateProductRequestDto>()))
            .Returns(new SmartCare.Domain.Entities.Product { ProductId = Guid.NewGuid() });

        var sut = new CreateProductCommandHandler(
            ResponseHandler, uow, uploader.Object,
            Mock.Of<IRedisCacheService>(), mapper.Object, Mock.Of<IBackgroundJobService>());

        var dto = new CreateProductRequestDto
        {
            NameEn = "Test", CategoryId = Guid.NewGuid(), CompanyId = Guid.NewGuid(),
            MainImage = Mock.Of<Microsoft.AspNetCore.Http.IFormFile>()
        };

        var result = await sut.Handle(new CreateProductCommand(dto), CT);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Contain("Main image upload failed");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenAllValid()
    {
        var category = new Category { Id = Guid.NewGuid() };
        var company = new Company { Id = Guid.NewGuid() };

        var categories = new Mock<ICategoryRepository>();
        categories.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), true)).ReturnsAsync(category);

        var companies = new Mock<ICompanyRepository>();
        companies.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), true)).ReturnsAsync(company);

        var products = new Mock<IProductRepository>();
        products.Setup(x => x.AddAsync(It.IsAny<SmartCare.Domain.Entities.Product>()))
            .ReturnsAsync((SmartCare.Domain.Entities.Product p) => p);

        var uploader = new Mock<IImageUploaderService>();
        uploader.Setup(x => x.UploadImageAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(), ImageFolder.ProductImages))
            .ReturnsAsync(new ImageUploadResult { Url = new Uri("https://cdn.test/img.png") });

        var mapper = new Mock<IMapper>();
        mapper.Setup(x => x.Map<SmartCare.Domain.Entities.Product>(It.IsAny<CreateProductRequestDto>()))
            .Returns(new SmartCare.Domain.Entities.Product { ProductId = Guid.NewGuid() });
        mapper.Setup(x => x.Map<ProductResponseDtoForAdmin>(It.IsAny<SmartCare.Domain.Entities.Product>()))
            .Returns(new ProductResponseDtoForAdmin { NameEn = "Test" });

        var uow = new UnitOfWorkMockBuilder()
            .WithCategories(categories.Object)
            .WithCompanies(companies.Object)
            .WithProducts(products.Object)
            .WithSaveChanges()
            .Build();

        var sut = new CreateProductCommandHandler(
            ResponseHandler, uow, uploader.Object,
            Mock.Of<IRedisCacheService>(), mapper.Object, Mock.Of<IBackgroundJobService>());

        var dto = new CreateProductRequestDto
        {
            NameEn = "Test", CategoryId = category.Id, CompanyId = company.Id,
            MainImage = Mock.Of<Microsoft.AspNetCore.Http.IFormFile>()
        };

        var result = await sut.Handle(new CreateProductCommand(dto), CT);

        result.Succeeded.Should().BeTrue();
        result.Message.Should().Be(SystemMessages.SUCCESS);
    }
}
