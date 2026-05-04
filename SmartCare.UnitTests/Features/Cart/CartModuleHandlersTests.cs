using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using SmartCare.Application.CQRs.Cart.Queries.GetCartById;
using SmartCare.Application.CQRs.Cart.Queries.GetCartItems;
using SmartCare.Application.CQRs.Cart.Queries.GetUserActiveCart;
using SmartCare.Application.DTOs.Cart.Requests;
using SmartCare.Application.DTOs.Cart.Responses;
using SmartCare.Application.Features.Carts.Commands.ClearCart;
using SmartCare.Application.Features.Carts.Commands.CreateCart;
using SmartCare.Application.Features.Carts.Commands.DeleteCart;
using SmartCare.Application.Features.Carts.Commands.RemoveItemFromCart;
using SmartCare.Application.Features.Carts.Commands.UpdateCartItem;
using SmartCare.Application.Features.Carts.Queries.GetUserActiveCart;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;

namespace SmartCare.UnitTests.Features;

public class CartModuleHandlersTests
{
    [Fact]
    public async Task ClearCart_ShouldReturnNotFound_WhenCartMissing()
    {
        var carts = new Mock<ICartRepository>();
        carts.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), false)).ReturnsAsync((Cart?)null);
        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Carts).Returns(carts.Object);

        var sut = new ClearCartCommandHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<ILogger<ClearCartCommandHandler>>(), uow.Object);
        var result = await sut.Handle(new ClearCartCommand(Guid.NewGuid()), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.NOT_FOUND);
    }

    [Fact]
    public async Task CreateCart_ShouldReturnBadRequest_WhenUserIdInvalid()
    {
        var sut = new CreateCartForUserCommandHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<IUnitOfWork>(), Mock.Of<ILogger<CreateCartForUserCommandHandler>>());
        var result = await sut.Handle(new CreateCartForUserCommand(""), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.BAD_REQUEST);
    }

    [Fact]
    public async Task CreateCart_ShouldReturnExistingCart_WhenAlreadyExists()
    {
        var existing = new Cart { Id = Guid.NewGuid(), ClientId = "u1" };
        var carts = new Mock<ICartRepository>();
        carts.Setup(x => x.GetActiveCartAsync("u1", false)).ReturnsAsync(existing);
        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Carts).Returns(carts.Object);

        var sut = new CreateCartForUserCommandHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<ILogger<CreateCartForUserCommandHandler>>());
        var result = await sut.Handle(new CreateCartForUserCommand("u1"), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().Be(existing.Id);
        result.Message.Should().Be(SystemMessages.CART_ALREADY_EXISTS);
    }

    [Fact]
    public async Task DeleteCart_ShouldReturnNotFound_WhenCartMissing()
    {
        var carts = new Mock<ICartRepository>();
        carts.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), false)).ReturnsAsync((Cart?)null);
        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Carts).Returns(carts.Object);

        var sut = new DeleteCartCommandHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<ILogger<DeleteCartCommandHandler>>(), uow.Object);
        var result = await sut.Handle(new DeleteCartCommand(Guid.NewGuid()), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.NOT_FOUND);
    }

    [Fact]
    public async Task RemoveFromCart_ShouldReturnNotFound_WhenItemMissing()
    {
        var cart = new Cart { Id = Guid.NewGuid(), ClientId = "u1" };
        var carts = new Mock<ICartRepository>();
        carts.Setup(x => x.GetByIdAsync(cart.Id, false)).ReturnsAsync(cart);
        carts.Setup(x => x.GetCartItemAsync(It.IsAny<Guid>(), false)).ReturnsAsync((CartItem?)null);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Carts).Returns(carts.Object);

        var sut = new RemoveFromCartCommandHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<ILogger<RemoveFromCartCommandHandler>>(), uow.Object);
        var dto = new RemoveFromCartRequestDto { CartId = cart.Id, CartItemId = Guid.NewGuid() };
        var result = await sut.Handle(new RemoveFromCartCommand(dto), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.NOT_FOUND);
    }

    [Fact]
    public async Task UpdateCartItem_ShouldReturnNotFound_WhenCartMissing()
    {
        var carts = new Mock<ICartRepository>();
        carts.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), true)).ReturnsAsync((Cart?)null);
        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Carts).Returns(carts.Object);

        var sut = new UpdateCartItemQuantityCommandHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<IMapper>(), Mock.Of<ILogger<UpdateCartItemQuantityCommandHandler>>(), uow.Object, Mock.Of<SmartCare.Application.IServices.IEventPublisherService>(), Mock.Of<SmartCare.Application.IServices.IBackgroundJobService>());
        var result = await sut.Handle(new UpdateCartItemQuantityCommand(new UpdateCartItemRequestDto { CartId = Guid.NewGuid(), CartItemId = Guid.NewGuid(), NewQuantity = 2 }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.CART_NOT_FOUND);
    }

    [Fact]
    public async Task UpdateCartItem_ShouldReturnBadRequest_WhenInsufficientStock()
    {
        var cart = new Cart { Id = Guid.NewGuid(), ClientId = "u1" };
        var item = new CartItem { CartItemId = Guid.NewGuid(), CartId = cart.Id, ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 10m };
        var product = new Product { ProductId = item.ProductId, Price = 10m };

        var carts = new Mock<ICartRepository>();
        carts.Setup(x => x.GetByIdAsync(cart.Id, true)).ReturnsAsync(cart);
        carts.Setup(x => x.GetCartItemAsync(item.CartItemId, true)).ReturnsAsync(item);

        var products = new Mock<IProductRepository>();
        products.Setup(x => x.GetByIdAsync(item.ProductId, false)).ReturnsAsync(product);

        var inventories = new Mock<IInventoryRepository>();
        inventories.Setup(x => x.GetTotalStockForProductAsync(product.ProductId)).ReturnsAsync(0);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Carts).Returns(carts.Object);
        uow.SetupGet(x => x.Products).Returns(products.Object);
        uow.SetupGet(x => x.Inventories).Returns(inventories.Object);

        var sut = new UpdateCartItemQuantityCommandHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<IMapper>(), Mock.Of<ILogger<UpdateCartItemQuantityCommandHandler>>(), uow.Object, Mock.Of<SmartCare.Application.IServices.IEventPublisherService>(), Mock.Of<SmartCare.Application.IServices.IBackgroundJobService>());
        var result = await sut.Handle(new UpdateCartItemQuantityCommand(new UpdateCartItemRequestDto { CartId = cart.Id, CartItemId = item.CartItemId, NewQuantity = 4 }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.INSUFFICIENT_STOCK);
    }

    [Fact]
    public async Task GetCartById_ShouldReturnBadRequest_WhenIdEmpty()
    {
        var sut = new GetCartByIdQueryHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<IMapper>(), Mock.Of<ILogger<GetCartByIdQueryHandler>>(), Mock.Of<IUnitOfWork>());
        var result = await sut.Handle(new GetCartByIdQuery(Guid.Empty), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.BAD_REQUEST);
    }

    [Fact]
    public async Task GetCartItems_ShouldReturnNotFound_WhenCartMissing()
    {
        var carts = new Mock<ICartRepository>();
        carts.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), false)).ReturnsAsync((Cart?)null);
        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Carts).Returns(carts.Object);

        var sut = new GetCartItemsQueryHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<IMapper>(), Mock.Of<ILogger<GetCartItemsQueryHandler>>(), uow.Object);
        var result = await sut.Handle(new GetCartItemsQuery(Guid.NewGuid()), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.NOT_FOUND);
    }

    [Fact]
    public async Task GetUserActiveCart_ShouldReturnBadRequest_WhenUserIdInvalid()
    {
        var sut = new GetUserActiveCartQueryHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<IMapper>(), Mock.Of<ILogger<GetUserActiveCartQueryHandler>>(), Mock.Of<IUnitOfWork>());
        var result = await sut.Handle(new GetUserActiveCartQuery(""), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.BAD_REQUEST);
    }

    [Fact]
    public async Task GetUserActiveCart_ShouldReturnNotFound_WhenNoActiveCart()
    {
        var carts = new Mock<ICartRepository>();
        carts.Setup(x => x.GetActiveCartAsync("u1", true)).ReturnsAsync((Cart?)null);
        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Carts).Returns(carts.Object);

        var sut = new GetUserActiveCartQueryHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<IMapper>(), Mock.Of<ILogger<GetUserActiveCartQueryHandler>>(), uow.Object);
        var result = await sut.Handle(new GetUserActiveCartQuery("u1"), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.NOT_FOUND);
    }
}
