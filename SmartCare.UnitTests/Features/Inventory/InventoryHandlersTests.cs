using AutoMapper;
using Moq;
using SmartCare.Application.CQRs.Inventory.Handlers;
using SmartCare.Application.CQRs.Inventory.Queries;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;

namespace SmartCare.UnitTests.Features;

public class InventoryHandlersTests
{
    [Fact]
    public async Task GetBestInventory_ShouldReturnBadRequest_WhenInvalidInput()
    {
        var uow = new Mock<IUnitOfWork>();
        var sut = new GetBestInventoryIdHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<IMapper>());

        var result = await sut.Handle(new GetBestInventoryIdQuery(Guid.Empty, 0), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.INVALID_INPUT);
    }

    [Fact]
    public async Task GetTotalStock_ShouldReturnBadRequest_WhenProductIdEmpty()
    {
        var uow = new Mock<IUnitOfWork>();
        var sut = new GetTotalStockForProductHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<IMapper>());

        var result = await sut.Handle(new GetTotalStockForProductQuery(Guid.Empty), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.INVALID_INPUT);
    }

    [Fact]
    public async Task GetTotalStock_ShouldReturnFailed_WhenProductNotFound()
    {
        var products = new Mock<IProductRepository>();
        products.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), false)).ReturnsAsync((Product?)null);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Products).Returns(products.Object);

        var sut = new GetTotalStockForProductHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<IMapper>());

        var result = await sut.Handle(new GetTotalStockForProductQuery(Guid.NewGuid()), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.PRODUCT_NOT_FOUND);
    }

    [Fact]
    public async Task GetBestInventory_ShouldReturnSuccess_WhenFound()
    {
        var productId = Guid.NewGuid();
        var inventories = new Mock<IInventoryRepository>();
        inventories.Setup(x => x.GetAvailableInventoryAsync(productId, 2)).ReturnsAsync(new Inventory { Id = Guid.NewGuid() });

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Inventories).Returns(inventories.Object);

        var sut = new GetBestInventoryIdHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<IMapper>());

        var result = await sut.Handle(new GetBestInventoryIdQuery(productId, 2), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task GetBestInventory_ShouldReturnFailed_WhenNotFound()
    {
        var productId = Guid.NewGuid();
        var inventories = new Mock<IInventoryRepository>();
        inventories.Setup(x => x.GetAvailableInventoryAsync(productId, 1)).ReturnsAsync((Inventory?)null);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Inventories).Returns(inventories.Object);

        var sut = new GetBestInventoryIdHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<IMapper>());

        var result = await sut.Handle(new GetBestInventoryIdQuery(productId, 1), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.NOT_FOUND);
    }
}
