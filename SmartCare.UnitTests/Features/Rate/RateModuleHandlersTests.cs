using AutoMapper;
using Moq;
using SmartCare.Application.CQRs.Rate.Commands;
using SmartCare.Application.CQRs.Rate.Handlers;
using SmartCare.Application.CQRs.Rate.Queries;
using SmartCare.Application.DTOs.Rates.Requests;
using SmartCare.Application.DTOs.Rates.Responses;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;

namespace SmartCare.UnitTests.Features;

public class RateModuleHandlersTests
{
    [Fact]
    public async Task CreateRate_ShouldFail_WhenUserMissing()
    {
        var clients = new Mock<IClientRepository>();
        clients.Setup(x => x.GetByIdAsync("u1", true)).ReturnsAsync((Client?)null);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Clients).Returns(clients.Object);

        var sut = new CreateRateHandler(uow.Object, Mock.Of<IRedisCacheService>(), Mock.Of<IMapper>(), new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler());
        var result = await sut.Handle(new CreateRateAsyncCommand("u1", new CreateRateRequestDto { ProductId = Guid.NewGuid(), Value = 5 }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.USER_NOT_FOUND);
    }

    [Fact]
    public async Task CreateRate_ShouldFail_WhenAlreadyRated()
    {
        var productId = Guid.NewGuid();
        var client = new Client { Id = "u1" };
        var product = new Product { ProductId = productId, NameEn = "Panadol" };

        var clients = new Mock<IClientRepository>();
        clients.Setup(x => x.GetByIdAsync("u1", true)).ReturnsAsync(client);
        var products = new Mock<IProductRepository>();
        products.Setup(x => x.GetByIdAsync(productId, false)).ReturnsAsync(product);
        var rates = new Mock<IRateRepository>();
        rates.Setup(x => x.IsProductRatedByUserAsync("u1", productId)).ReturnsAsync(true);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Clients).Returns(clients.Object);
        uow.SetupGet(x => x.Products).Returns(products.Object);
        uow.SetupGet(x => x.Rates).Returns(rates.Object);

        var sut = new CreateRateHandler(uow.Object, Mock.Of<IRedisCacheService>(), Mock.Of<IMapper>(), new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler());
        var result = await sut.Handle(new CreateRateAsyncCommand("u1", new CreateRateRequestDto { ProductId = productId, Value = 5 }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.RATE_ALREADY_EXISTS);
    }

    [Fact]
    public async Task DeleteRate_ShouldFail_WhenInputInvalid()
    {
        var sut = new DeleteRateHandler(Mock.Of<IUnitOfWork>(), Mock.Of<IRedisCacheService>(), Mock.Of<IMapper>(), new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler());
        var result = await sut.Handle(new DeleteRateAsyncCommand("", Guid.Empty), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.INVALID_INPUT);
    }

    [Fact]
    public async Task DeleteRate_ShouldFail_WhenRateMissing()
    {
        var client = new Client { Id = "u1" };
        var clients = new Mock<IClientRepository>();
        clients.Setup(x => x.GetByIdAsync("u1", true)).ReturnsAsync(client);

        var rates = new Mock<IRateRepository>();
        rates.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), true)).ReturnsAsync((Rate?)null);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Clients).Returns(clients.Object);
        uow.SetupGet(x => x.Rates).Returns(rates.Object);

        var sut = new DeleteRateHandler(uow.Object, Mock.Of<IRedisCacheService>(), Mock.Of<IMapper>(), new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler());
        var result = await sut.Handle(new DeleteRateAsyncCommand("u1", Guid.NewGuid()), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.RATE_NOT_FOUND);
    }

    [Fact]
    public async Task GetRatesForProduct_ShouldFail_WhenIdInvalid()
    {
        var sut = new GetAllRatesForProductHandler(Mock.Of<IUnitOfWork>(), Mock.Of<IRedisCacheService>(), Mock.Of<IMapper>(), new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler());
        var result = await sut.Handle(new GetAllRatesForProductAsyncQuery(Guid.Empty), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.INVALID_INPUT);
    }

    [Fact]
    public async Task GetRatesForProduct_ShouldReturnCached_WhenCacheHit()
    {
        var cache = new Mock<IRedisCacheService>();
        cache.Setup(x => x.GetDataAsync<IEnumerable<RateResponseDto>>(It.IsAny<string>(), CacheConstants.Rates))
            .ReturnsAsync(new List<RateResponseDto> { new RateResponseDto { Id = Guid.NewGuid() } });

        var sut = new GetAllRatesForProductHandler(Mock.Of<IUnitOfWork>(), cache.Object, Mock.Of<IMapper>(), new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler());
        var result = await sut.Handle(new GetAllRatesForProductAsyncQuery(Guid.NewGuid()), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetRatesForUser_ShouldFail_WhenUserIdInvalid()
    {
        var sut = new GetAllRatesForUserHandler(Mock.Of<IUnitOfWork>(), Mock.Of<IRedisCacheService>(), Mock.Of<IMapper>(), new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler());
        var result = await sut.Handle(new GetAllRatesForUserAsyncQuery(""), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.INVALID_INPUT);
    }

    [Fact]
    public async Task GetRateById_ShouldFail_WhenRateMissing()
    {
        var rates = new Mock<IRateRepository>();
        rates.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), false)).ReturnsAsync((Rate?)null);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Rates).Returns(rates.Object);

        var sut = new GetRateByIdHandler(uow.Object, Mock.Of<IRedisCacheService>(), Mock.Of<IMapper>(), new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler());
        var result = await sut.Handle(new GetRateByIdAsyncQuery(Guid.NewGuid()), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.NOT_FOUND);
    }

    [Fact]
    public async Task UpdateRate_ShouldFail_WhenNotOwner()
    {
        var rateId = Guid.NewGuid();
        var user = new Client { Id = "u1" };
        var rate = new Rate { Id = rateId, ClientId = "other", ProductId = Guid.NewGuid(), Value = 3 };

        var clients = new Mock<IClientRepository>();
        clients.Setup(x => x.GetByIdAsync("u1", false)).ReturnsAsync(user);

        var rates = new Mock<IRateRepository>();
        rates.Setup(x => x.GetByIdAsync(rateId, true)).ReturnsAsync(rate);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Clients).Returns(clients.Object);
        uow.SetupGet(x => x.Rates).Returns(rates.Object);

        var sut = new UpdateRateHandler(uow.Object, Mock.Of<IRedisCacheService>(), Mock.Of<IMapper>(), new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler());
        var result = await sut.Handle(new UpdateRateAsyncCommand("u1", new UpdateRateRequestDto { Id = rateId, ProductId = rate.ProductId, Value = 4 }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.UNAUTHORIZED);
    }
}
