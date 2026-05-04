using SmartCare.Application.Features.Product.Commands.Restore;
using SmartCare.Application.IServices;

namespace SmartCare.UnitTests.Features.Products;

public class RestoreProductCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenIdIsEmpty()
    {
        var sut = new RestoreProductCommandHandler(
            ResponseHandler, Mock.Of<IUnitOfWork>(), Mock.Of<IRedisCacheService>());

        var result = await sut.Handle(new RestoreProductCommand(Guid.Empty), CT);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.INVALID_INPUT);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenProductMissing()
    {
        var products = new Mock<IProductRepository>();
        products.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), true))
            .ReturnsAsync((SmartCare.Domain.Entities.Product?)null);

        var uow = new UnitOfWorkMockBuilder().WithProducts(products.Object).Build();

        var sut = new RestoreProductCommandHandler(ResponseHandler, uow, Mock.Of<IRedisCacheService>());

        var result = await sut.Handle(new RestoreProductCommand(Guid.NewGuid()), CT);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.NOT_FOUND);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenProductRestored()
    {
        var product = new SmartCare.Domain.Entities.Product { ProductId = Guid.NewGuid(), IsDeleted = true };

        var products = new Mock<IProductRepository>();
        products.Setup(x => x.GetByIdAsync(product.ProductId, true)).ReturnsAsync(product);

        var uow = new UnitOfWorkMockBuilder()
            .WithProducts(products.Object)
            .WithSaveChanges()
            .Build();

        var sut = new RestoreProductCommandHandler(ResponseHandler, uow, Mock.Of<IRedisCacheService>());

        var result = await sut.Handle(new RestoreProductCommand(product.ProductId), CT);

        result.Succeeded.Should().BeTrue();
    }
}
