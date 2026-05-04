using AutoMapper;
using Moq;
using SmartCare.Application.CQRs.Inventory.Commands;
using SmartCare.Application.CQRs.Inventory.Handlers;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;

namespace SmartCare.UnitTests.Features;

public class InventoryCommandHandlersTests
{
    [Fact]
    public async Task ReserveStock_ShouldReturnBadRequest_WhenInvalidInput()
    {
        var sut = new ReserveStockHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), new Mock<IUnitOfWork>().Object, Mock.Of<IMapper>());

        var result = await sut.Handle(new ReserveStockAsyncCommand(Guid.Empty, 0), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.INVALID_INPUT);
    }

    [Fact]
    public async Task ReserveStock_ShouldReturnSuccess_WhenRepositoryReturnsTrue()
    {
        var inventories = new Mock<IInventoryRepository>();
        inventories.Setup(x => x.ReserveStockAsync(It.IsAny<Guid>(), 2)).ReturnsAsync(true);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Inventories).Returns(inventories.Object);

        var sut = new ReserveStockHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<IMapper>());
        var result = await sut.Handle(new ReserveStockAsyncCommand(Guid.NewGuid(), 2), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Message.Should().Be(SystemMessages.INVENTORY_UPDATED);
    }

    [Fact]
    public async Task ReleaseReservedStock_ShouldReturnFailed_WhenRepositoryReturnsFalse()
    {
        var inventories = new Mock<IInventoryRepository>();
        inventories.Setup(x => x.ReleaseReservedStockAsync(It.IsAny<Guid>(), 2)).ReturnsAsync(false);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Inventories).Returns(inventories.Object);

        var sut = new ReleaseReservedStockHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<IMapper>());
        var result = await sut.Handle(new ReleaseReservedStockAsyncCommand(Guid.NewGuid(), 2), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.FAILED);
    }

    [Fact]
    public async Task SetStockLevel_ShouldReturnSuccess_WhenRepositoryReturnsTrue()
    {
        var inventories = new Mock<IInventoryRepository>();
        inventories.Setup(x => x.SetStockLevelAsync(It.IsAny<Guid>(), 10)).ReturnsAsync(true);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Inventories).Returns(inventories.Object);

        var sut = new SetStockLevelHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<IMapper>());
        var result = await sut.Handle(new SetStockLevelAsyncCommand(Guid.NewGuid(), 10), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task TransferStock_ShouldReturnFailed_WhenFromInventoryMissing()
    {
        var inventories = new Mock<IInventoryRepository>();
        inventories.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), false)).ReturnsAsync((Inventory?)null);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Inventories).Returns(inventories.Object);

        var sut = new TransferStockHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<IMapper>());
        var result = await sut.Handle(new TransferStockAsyncCommand(Guid.NewGuid(), Guid.NewGuid(), 1), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.INVENTORY_NOT_FOUND);
    }

    [Fact]
    public async Task TransferStock_ShouldThrow_WhenQuantityExceedsAvailable()
    {
        var from = new Inventory { Id = Guid.NewGuid(), StockQuantity = 3, ReservedQuantity = 2 };
        var to = new Inventory { Id = Guid.NewGuid(), StockQuantity = 1, ReservedQuantity = 0 };

        var inventories = new Mock<IInventoryRepository>();
        inventories.SetupSequence(x => x.GetByIdAsync(It.IsAny<Guid>(), false)).ReturnsAsync(from).ReturnsAsync(to);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Inventories).Returns(inventories.Object);

        var sut = new TransferStockHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<IMapper>());

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.Handle(new TransferStockAsyncCommand(from.Id, to.Id, 2), CancellationToken.None));
    }
}
