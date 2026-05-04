using AutoMapper;
using Moq;
using SmartCare.Application.CQRs.Favourite.Commands;
using SmartCare.Application.CQRs.Favourite.Handlers;
using SmartCare.Application.CQRs.Favourite.Queries;
using SmartCare.Application.DTOs.Favorites.Requests;
using SmartCare.Application.DTOs.Favorites.Responses;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;

namespace SmartCare.UnitTests.Features;

public class FavouriteModuleHandlersTests
{
    [Fact]
    public async Task CreateFavourite_ShouldFail_WhenUserMissing()
    {
        var clients = new Mock<IClientRepository>();
        clients.Setup(x => x.GetByIdAsync("u1", true)).ReturnsAsync((Client?)null);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Clients).Returns(clients.Object);

        var sut = new CreateFavouriteHandler(uow.Object, Mock.Of<IMapper>(), new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<IRedisCacheService>());
        var result = await sut.Handle(new CreateFavouriteAsyncCommand(new CreateFavouriteRequestDto { ClientId = "u1", ProductId = Guid.NewGuid() }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.USER_NOT_FOUND);
    }

    [Fact]
    public async Task CreateFavourite_ShouldFail_WhenAlreadyExists()
    {
        var productId = Guid.NewGuid();
        var dto = new CreateFavouriteRequestDto { ClientId = "u1", ProductId = productId };

        var clients = new Mock<IClientRepository>();
        clients.Setup(x => x.GetByIdAsync("u1", true)).ReturnsAsync(new Client { Id = "u1" });

        var products = new Mock<IProductRepository>();
        products.Setup(x => x.GetByIdAsync(productId, false)).ReturnsAsync(new Product { ProductId = productId });

        var favs = new Mock<IFavouriteRepository>();
        favs.Setup(x => x.IsProductFavoritedByUserAsync("u1", productId)).ReturnsAsync(true);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Clients).Returns(clients.Object);
        uow.SetupGet(x => x.Products).Returns(products.Object);
        uow.SetupGet(x => x.Favourites).Returns(favs.Object);

        var sut = new CreateFavouriteHandler(uow.Object, Mock.Of<IMapper>(), new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<IRedisCacheService>());
        var result = await sut.Handle(new CreateFavouriteAsyncCommand(dto), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.FAVOURITE_ALREADY_EXISTS);
    }

    [Fact]
    public async Task DeleteFavourite_ShouldFail_WhenInvalidInput()
    {
        var sut = new DeleteFavouriteHandler(Mock.Of<IUnitOfWork>(), Mock.Of<IMapper>(), new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<IRedisCacheService>());
        var result = await sut.Handle(new DeleteFavouriteAsyncCommand("", Guid.Empty), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.INVALID_INPUT);
    }

    [Fact]
    public async Task DeleteFavourite_ShouldFail_WhenFavoriteMissing()
    {
        var clients = new Mock<IClientRepository>();
        clients.Setup(x => x.GetByIdAsync("u1", false)).ReturnsAsync(new Client { Id = "u1" });

        var favs = new Mock<IFavouriteRepository>();
        favs.Setup(x => x.CheackFavouriteExistsAsync("u1", It.IsAny<Guid>())).ReturnsAsync((Favorite?)null);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Clients).Returns(clients.Object);
        uow.SetupGet(x => x.Favourites).Returns(favs.Object);

        var sut = new DeleteFavouriteHandler(uow.Object, Mock.Of<IMapper>(), new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<IRedisCacheService>());
        var result = await sut.Handle(new DeleteFavouriteAsyncCommand("u1", Guid.NewGuid()), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.NOT_FOUND);
    }

    [Fact]
    public async Task GetAllFavourites_ShouldFail_WhenUserIdInvalid()
    {
        var sut = new GetAllFavouritesForUserHandler(Mock.Of<IUnitOfWork>(), Mock.Of<IMapper>(), new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<IRedisCacheService>());
        var result = await sut.Handle(new GetAllFavouritesForUserAsyncQuery(""), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.INVALID_INPUT);
    }

    [Fact]
    public async Task GetAllFavourites_ShouldReturnCached_WhenCacheHit()
    {
        var cache = new Mock<IRedisCacheService>();
        cache.Setup(x => x.GetDataAsync<IEnumerable<FavoriteResponseDto>>(It.IsAny<string>(), CacheConstants.Favourite))
            .ReturnsAsync(new List<FavoriteResponseDto> { new FavoriteResponseDto() });

        var sut = new GetAllFavouritesForUserHandler(Mock.Of<IUnitOfWork>(), Mock.Of<IMapper>(), new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), cache.Object);
        var result = await sut.Handle(new GetAllFavouritesForUserAsyncQuery("u1"), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }
}
