using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SmartCare.Application.commens;
using SmartCare.Application.DTOs.Orders.Requests;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.ExternalServiceInterfaces.Payments;
using SmartCare.Application.Features.Orders.Commands.CreateOnlineOrder;
using SmartCare.Application.Features.Orders.Commands.CreatePickUpOrder;
using SmartCare.Application.Features.Orders.Commands.UpdateOrder;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;

namespace SmartCare.UnitTests.Features;

public class OrderOrchestratorHandlersTests
{
    private static IConfiguration BuildConfig() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ReservationTimes:ForOrderExpirationMinutes"] = "15"
        })
        .Build();

    [Fact]
    public async Task CreateOnlineOrder_ShouldReturnBadRequest_WhenClientMissing()
    {
        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Clients).Returns(Mock.Of<IClientRepository>());

        var sut = new CreateOnlineOrderCommandHandler(
            BuildConfig(),
            new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(),
            uow.Object,
            Mock.Of<IBackgroundJobService>(),
            Mock.Of<IMapper>(),
            Mock.Of<ISqlLockManager>(),
            Mock.Of<ILogger<CreateOnlineOrderCommandHandler>>(),
            Mock.Of<IEventPublisherService>());

        var result = await sut.Handle(new CreateOnlineOrderFromCartAsyncCommand("c1", new CreateOnlineOrderRequestDto { CartId = Guid.NewGuid(), deliveryAddressId = Guid.NewGuid() }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.USER_NOT_FOUND);
    }

    [Fact]
    public async Task CreateOnlineOrder_ShouldReturnBadRequest_WhenCartEmpty()
    {
        var cart = new Cart { Id = Guid.NewGuid(), ClientId = "c1", Items = new List<CartItem>() };

        var clients = new Mock<IClientRepository>();
        clients.Setup(x => x.GetByIdAsync("c1", false)).ReturnsAsync(new Client());

        var carts = new Mock<ICartRepository>();
        carts.Setup(x => x.GetByIdAsync(cart.Id, true)).ReturnsAsync(cart);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Clients).Returns(clients.Object);
        uow.SetupGet(x => x.Carts).Returns(carts.Object);

        var sut = new CreateOnlineOrderCommandHandler(BuildConfig(), new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<IBackgroundJobService>(), Mock.Of<IMapper>(), Mock.Of<ISqlLockManager>(), Mock.Of<ILogger<CreateOnlineOrderCommandHandler>>(), Mock.Of<IEventPublisherService>());

        var result = await sut.Handle(new CreateOnlineOrderFromCartAsyncCommand("c1", new CreateOnlineOrderRequestDto { CartId = cart.Id, deliveryAddressId = Guid.NewGuid() }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.CART_EMPTY);
    }

    [Fact]
    public async Task CreateOnlineOrder_ShouldReturnStockPayload_WhenInventoryUnavailableInSoftValidation()
    {
        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            ClientId = "c1",
            Items = new List<CartItem> { new CartItem { ProductId = Guid.NewGuid(), Quantity = 2 } }
        };

        var clients = new Mock<IClientRepository>();
        clients.Setup(x => x.GetByIdAsync("c1", false)).ReturnsAsync(new Client());

        var carts = new Mock<ICartRepository>();
        carts.Setup(x => x.GetByIdAsync(cart.Id, true)).ReturnsAsync(cart);

        var inventories = new Mock<IInventoryRepository>();
        inventories.Setup(x => x.GetAvailableInventoryAsync(It.IsAny<Guid>(), It.IsAny<int>())).ReturnsAsync((Inventory?)null);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Clients).Returns(clients.Object);
        uow.SetupGet(x => x.Carts).Returns(carts.Object);
        uow.SetupGet(x => x.Inventories).Returns(inventories.Object);

        var sut = new CreateOnlineOrderCommandHandler(BuildConfig(), new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<IBackgroundJobService>(), Mock.Of<IMapper>(), Mock.Of<ISqlLockManager>(), Mock.Of<ILogger<CreateOnlineOrderCommandHandler>>(), Mock.Of<IEventPublisherService>());

        var result = await sut.Handle(new CreateOnlineOrderFromCartAsyncCommand("c1", new CreateOnlineOrderRequestDto { CartId = cart.Id, deliveryAddressId = Guid.NewGuid() }), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.outOfStocks.Should().NotBeNull();
    }

    [Fact]
    public async Task CreatePickupOrder_ShouldReturnBadRequest_WhenCartNotOwnedByClient()
    {
        var cart = new Cart { Id = Guid.NewGuid(), ClientId = "other", Items = new List<CartItem>() };

        var clients = new Mock<IClientRepository>();
        clients.Setup(x => x.GetByIdAsync("c1", false)).ReturnsAsync(new Client());

        var carts = new Mock<ICartRepository>();
        carts.Setup(x => x.GetByIdAsync(cart.Id, true)).ReturnsAsync(cart);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Clients).Returns(clients.Object);
        uow.SetupGet(x => x.Carts).Returns(carts.Object);

        var sut = new CreatePickupOrderFromCartAsyncHandler(BuildConfig(), new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<IBackgroundJobService>(), Mock.Of<IMapper>(), Mock.Of<ISqlLockManager>(), Mock.Of<ILogger<CreatePickupOrderFromCartAsyncHandler>>(), Mock.Of<IEventPublisherService>());

        var result = await sut.Handle(new CreatePickupOrderFromCartCommand("c1", new CreatePickUpOrderRequestDto { CartId = cart.Id, storeId = Guid.NewGuid() }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.CART_NOT_FOUND);
    }

    [Fact]
    public async Task CreatePickupOrder_ShouldReturnStockError_WhenStoreInventoryMissing()
    {
        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            ClientId = "c1",
            Items = new List<CartItem> { new CartItem { ProductId = Guid.NewGuid(), Quantity = 1 } }
        };

        var clients = new Mock<IClientRepository>();
        clients.Setup(x => x.GetByIdAsync("c1", false)).ReturnsAsync(new Client());

        var carts = new Mock<ICartRepository>();
        carts.Setup(x => x.GetByIdAsync(cart.Id, true)).ReturnsAsync(cart);

        var inventories = new Mock<IInventoryRepository>();
        inventories.Setup(x => x.GetStockOfProductInStoreAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>())).ReturnsAsync((Inventory?)null);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Clients).Returns(clients.Object);
        uow.SetupGet(x => x.Carts).Returns(carts.Object);
        uow.SetupGet(x => x.Inventories).Returns(inventories.Object);

        var sut = new CreatePickupOrderFromCartAsyncHandler(BuildConfig(), new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<IBackgroundJobService>(), Mock.Of<IMapper>(), Mock.Of<ISqlLockManager>(), Mock.Of<ILogger<CreatePickupOrderFromCartAsyncHandler>>(), Mock.Of<IEventPublisherService>());

        var result = await sut.Handle(new CreatePickupOrderFromCartCommand("c1", new CreatePickUpOrderRequestDto { CartId = cart.Id, storeId = Guid.NewGuid() }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Data!.outOfStocks.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateOrder_ShouldReturnBadRequest_WhenOrderNotPending()
    {
        var cart = new Cart { Id = Guid.NewGuid(), ClientId = "c1" };
        var cartItems = new List<CartItem> { new CartItem { ProductId = Guid.NewGuid(), Quantity = 1, InventoryId = Guid.NewGuid() } };
        var order = new Order { Id = Guid.NewGuid(), ClientId = "c1", Status = OrderStatus.Confirmed };

        var clients = new Mock<IClientRepository>();
        clients.Setup(x => x.GetByIdAsync("c1", false)).ReturnsAsync(new Client());

        var carts = new Mock<ICartRepository>();
        carts.Setup(x => x.GetByIdAsync(cart.Id, true)).ReturnsAsync(cart);
        carts.Setup(x => x.GetCartItemsAsync(cart.Id)).ReturnsAsync(cartItems);

        var orders = new Mock<IOrderRepository>();
        orders.Setup(x => x.GetOrderWithDetailsByIdAsync(order.Id, true)).ReturnsAsync(order);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Clients).Returns(clients.Object);
        uow.SetupGet(x => x.Carts).Returns(carts.Object);
        uow.SetupGet(x => x.Orders).Returns(orders.Object);

        var sut = new UpdateOrderCommandHandler(BuildConfig(), new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<IBackgroundJobService>(), Mock.Of<IMapper>(), Mock.Of<ISqlLockManager>(), Mock.Of<ILogger<UpdateOrderCommandHandler>>(), Mock.Of<IPaymentGatewayFactory>(), Mock.Of<IEventPublisherService>());

        var result = await sut.Handle(new UpdateOrderCommand("c1", new UpdateOrderRequestDto { OrderId = order.Id, CartId = cart.Id, UpdatedOrderType = OrderType.Online }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.ORDER_NOT_EDITABLE);
    }
}
