using SmartCare.Application.DTOs.Product.Requests;
using SmartCare.Application.DTOs.Product.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Features.Product.Commands.Update;
using SmartCare.Application.IServices;

namespace SmartCare.UnitTests.Features.Products;

public class UpdateProductCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenProductMissing()
    {
        var products = new Mock<IProductRepository>();
        products.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), true))
            .ReturnsAsync((SmartCare.Domain.Entities.Product?)null);

        var uow = new UnitOfWorkMockBuilder().WithProducts(products.Object).Build();

        var sut = new UpdateProductCommandHandler(
            ResponseHandler, uow, Mock.Of<IImageUploaderService>(),
            Mock.Of<IRedisCacheService>(), Mock.Of<IMapper>());

        var dto = new UpdateProductRequestDto { ProductId = Guid.NewGuid(), NameEn = "x" };
        var result = await sut.Handle(new UpdateProductCommand(dto), CT);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Contain("Product not found");
    }

    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenNewCategoryInvalid()
    {
        var existingProduct = new SmartCare.Domain.Entities.Product
        {
            ProductId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            CompanyId = Guid.NewGuid(),
            Images = new List<ProductImage>()
        };

        var products = new Mock<IProductRepository>();
        products.Setup(x => x.GetByIdAsync(existingProduct.ProductId, true)).ReturnsAsync(existingProduct);

        var categories = new Mock<ICategoryRepository>();
        categories.Setup(x => x.GetByIdAsync(existingProduct.CategoryId, true))
            .ReturnsAsync(new Category { Id = existingProduct.CategoryId });
        categories.Setup(x => x.GetByIdAsync(It.Is<Guid>(id => id != existingProduct.CategoryId), false))
            .ReturnsAsync((Category?)null);

        var mapper = new Mock<IMapper>();

        var uow = new UnitOfWorkMockBuilder()
            .WithProducts(products.Object)
            .WithCategories(categories.Object)
            .Build();

        var sut = new UpdateProductCommandHandler(
            ResponseHandler, uow, Mock.Of<IImageUploaderService>(),
            Mock.Of<IRedisCacheService>(), mapper.Object);

        var newCatId = Guid.NewGuid();
        var dto = new UpdateProductRequestDto
        {
            ProductId = existingProduct.ProductId,
            NameEn = "Updated",
            CategoryId = newCatId
        };

        var result = await sut.Handle(new UpdateProductCommand(dto), CT);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Contain("Invalid category");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenBasicFieldsUpdated()
    {
        var existingProduct = new SmartCare.Domain.Entities.Product
        {
            ProductId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            CompanyId = Guid.NewGuid(),
            NameEn = "Old",
            Images = new List<ProductImage>()
        };

        var products = new Mock<IProductRepository>();
        products.Setup(x => x.GetByIdAsync(existingProduct.ProductId, true)).ReturnsAsync(existingProduct);

        var mapper = new Mock<IMapper>();
        mapper.Setup(x => x.Map(It.IsAny<UpdateProductRequestDto>(), existingProduct));
        mapper.Setup(x => x.Map<ProductResponseDtoForAdmin>(existingProduct))
            .Returns(new ProductResponseDtoForAdmin { NameEn = "Updated" });

        var uow = new UnitOfWorkMockBuilder()
            .WithProducts(products.Object)
            .WithSaveChanges()
            .Build();

        var sut = new UpdateProductCommandHandler(
            ResponseHandler, uow, Mock.Of<IImageUploaderService>(),
            Mock.Of<IRedisCacheService>(), mapper.Object);

        var dto = new UpdateProductRequestDto { ProductId = existingProduct.ProductId, NameEn = "Updated" };
        var result = await sut.Handle(new UpdateProductCommand(dto), CT);

        result.Succeeded.Should().BeTrue();
        result.Message.Should().Contain("Updated successfully");
    }
}
