using SmartCare.Application.Features.Product.Commands.Delete;
using SmartCare.Application.IServices;

namespace SmartCare.UnitTests.Features.Products;

public class DeleteProductCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenIdIsEmpty()
    {
        var sut = new DeleteProductCommandHandler(
            ResponseHandler, Mock.Of<IUnitOfWork>(), Mock.Of<IRedisCacheService>());

        var result = await sut.Handle(new DeleteProductCommand(Guid.Empty), CT);

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

        var sut = new DeleteProductCommandHandler(ResponseHandler, uow, Mock.Of<IRedisCacheService>());

        var result = await sut.Handle(new DeleteProductCommand(Guid.NewGuid()), CT);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.NOT_FOUND);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenProductDeleted()
    {
        var product = new SmartCare.Domain.Entities.Product { ProductId = Guid.NewGuid() };

        var products = new Mock<IProductRepository>();
        products.Setup(x => x.GetByIdAsync(product.ProductId, true)).ReturnsAsync(product);

        var uow = new UnitOfWorkMockBuilder()
            .WithProducts(products.Object)
            .WithSaveChanges()
            .Build();

        var sut = new DeleteProductCommandHandler(ResponseHandler, uow, Mock.Of<IRedisCacheService>());

        var result = await sut.Handle(new DeleteProductCommand(product.ProductId), CT);

        result.Succeeded.Should().BeTrue();
        result.Message.Should().Be(SystemMessages.RECORD_DELETED);
    }
}
