using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using SmartCare.Application.DTOs.Cart.Requests;
using SmartCare.Application.DTOs.Cart.Responses;
using SmartCare.Application.Features.Carts.Commands.AddToCart;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;

namespace SmartCare.UnitTests.Features;

public class AddToCartCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenCartMissing()
    {
        var cartRepo = new Mock<ICartRepository>();
        cartRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), true)).ReturnsAsync((Cart?)null);

        var uow = BuildUow(cartRepo: cartRepo.Object);
        var sut = BuildSut(uow);

        var result = await sut.Handle(new AddToCartCommand(new AddToCartRequestDto { CartId = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 1 }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.CART_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenInsufficientStock()
    {
        var cart = new Cart { Id = Guid.NewGuid(), ClientId = "client-1" };
        var product = new Product { ProductId = Guid.NewGuid(), Price = 100m };

        var cartRepo = new Mock<ICartRepository>();
        cartRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), true)).ReturnsAsync(cart);
        cartRepo.Setup(x => x.CheckIfProductExistInCart(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(false);

        var productRepo = new Mock<IProductRepository>();
        productRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), false)).ReturnsAsync(product);

        var inventoryRepo = new Mock<IInventoryRepository>();
        inventoryRepo.Setup(x => x.GetTotalStockForProductAsync(product.ProductId)).ReturnsAsync(1);

        var uow = BuildUow(cartRepo.Object, productRepo.Object, inventoryRepo.Object);
        var sut = BuildSut(uow);

        var result = await sut.Handle(new AddToCartCommand(new AddToCartRequestDto { CartId = cart.Id, ProductId = product.ProductId, Quantity = 3 }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.INSUFFICIENT_STOCK);
    }

    private static IUnitOfWork BuildUow(ICartRepository? cartRepo = null, IProductRepository? productRepo = null, IInventoryRepository? inventoryRepo = null)
    {
        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Carts).Returns(cartRepo ?? Mock.Of<ICartRepository>());
        uow.SetupGet(x => x.Products).Returns(productRepo ?? Mock.Of<IProductRepository>());
        uow.SetupGet(x => x.Inventories).Returns(inventoryRepo ?? Mock.Of<IInventoryRepository>());
        return uow.Object;
    }

    private static AddToCartCommandHandler BuildSut(IUnitOfWork uow)
    {
        var responseHandler = new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler();
        var mapper = new Mock<IMapper>();
        mapper.Setup(x => x.Map<CartItem>(It.IsAny<AddToCartRequestDto>())).Returns(new CartItem { Quantity = 1 });
        mapper.Setup(x => x.Map<CartItemResponseDto?>(It.IsAny<CartItem>())).Returns(new CartItemResponseDto());

        return new AddToCartCommandHandler(
            responseHandler,
            mapper.Object,
            Mock.Of<ILogger<AddToCartCommandHandler>>(),
            uow,
            Mock.Of<IEventPublisherService>(),
            Mock.Of<IBackgroundJobService>());
    }
}
