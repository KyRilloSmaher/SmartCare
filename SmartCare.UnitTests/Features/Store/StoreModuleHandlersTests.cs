using AutoMapper;
using Moq;
using SmartCare.Application.DTOs.Stores.Requests;
using SmartCare.Application.DTOs.Stores.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Features.Store.Commands.Create;
using SmartCare.Application.Features.Store.Commands.Delete;
using SmartCare.Application.Features.Store.Commands.Update;
using SmartCare.Application.Features.Store.Queries.GetAll;
using SmartCare.Application.Features.Store.Queries.GetAllForAdmin;
using SmartCare.Application.Features.Store.Queries.GetById;
using SmartCare.Application.Features.Store.Queries.GetNearest;
using SmartCare.Application.Features.Store.Queries.Search;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;

namespace SmartCare.UnitTests.Features;

public class StoreModuleHandlersTests
{
    [Fact]
    public async Task UpdateStore_ShouldReturnBadRequest_WhenInvalidInput()
    {
        var sut = new UpdateStoreCommandHandler(Mock.Of<IUnitOfWork>(), Mock.Of<IRedisCacheService>(), Mock.Of<IMapper>(), Mock.Of<IMapService>(), new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler());
        var result = await sut.Handle(new UpdateStoreCommand(new UpdateStoreRequestDto { Id = Guid.Empty }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.INVALID_INPUT);
    }

    [Fact]
    public async Task DeleteStore_ShouldReturnNotFound_WhenMissing()
    {
        var stores = new Mock<IStoreRepository>();
        stores.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), true)).ReturnsAsync((Store?)null);
        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Stores).Returns(stores.Object);

        var sut = new DeleteStoreCommandHandler(uow.Object, Mock.Of<IRedisCacheService>(), new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler());
        var result = await sut.Handle(new DeleteStoreCommand(Guid.NewGuid()), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.NOT_FOUND);
    }

    [Fact]
    public async Task GetAllStores_ShouldReturnCached_WhenCacheHit()
    {
        var cache = new Mock<IRedisCacheService>();
        cache.Setup(x => x.GetDataAsync<IEnumerable<StoreResponseDto>>("stores_client_all", CacheConstants.Stories))
            .ReturnsAsync(new List<StoreResponseDto> { new StoreResponseDto { Id = Guid.NewGuid() } });

        var sut = new GetAllStoresQueryHandler(Mock.Of<IUnitOfWork>(), cache.Object, Mock.Of<IMapper>(), Mock.Of<IMapService>(), new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler());
        var result = await sut.Handle(new GetAllStoresQuery(), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetStoreById_ShouldReturnBadRequest_WhenIdEmpty()
    {
        var sut = new GetStoreByIdQueryHandler(Mock.Of<IUnitOfWork>(), Mock.Of<IRedisCacheService>(), Mock.Of<IMapper>(), Mock.Of<IMapService>(), new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler());
        var result = await sut.Handle(new GetStoreByIdQuery(Guid.Empty), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.INVALID_INPUT);
    }

    [Fact]
    public async Task GetNearestStore_ShouldReturnNotFound_WhenNoStores()
    {
        var stores = new Mock<IStoreRepository>();
        stores.Setup(x => x.GetAllStoresAsync()).ReturnsAsync(new List<Store>());
        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Stores).Returns(stores.Object);

        var sut = new GetNearestStoreQueryHandler(uow.Object, Mock.Of<IRedisCacheService>(), Mock.Of<IMapper>(), Mock.Of<IMapService>(), new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler());
        var result = await sut.Handle(new GetNearestStoreQuery(new AddressValuesDto { Latitude = 0, Longitude = 0 }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.NOT_FOUND);
    }

    [Fact]
    public async Task SearchStores_ShouldReturnBadRequest_WhenNameEmpty()
    {
        var sut = new SearchStoresByNameQueryHandler(Mock.Of<IUnitOfWork>(), Mock.Of<IRedisCacheService>(), Mock.Of<IMapper>(), Mock.Of<IMapService>(), new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler());
        var result = await sut.Handle(new SearchStoresByNameQuery(""), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.INVALID_INPUT);
    }
}
