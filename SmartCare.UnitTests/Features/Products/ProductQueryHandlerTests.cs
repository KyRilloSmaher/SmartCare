using SmartCare.Application.CQRs.Product.Handlers;
using SmartCare.Application.CQRs.Product.Queries;
using SmartCare.Application.DTOs.Product.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Projection_Models;

namespace SmartCare.UnitTests.Features.Products;

public class ProductQueryHandlerTests : TestBase
{
    #region GetAllProducts

    [Fact]
    public async Task GetAllProducts_ShouldReturnBadRequest_WhenPaginationInvalid()
    {
        var sut = new GetAllProductsHandler(
            ResponseHandler, Mock.Of<IUnitOfWork>(), Mock.Of<IImageUploaderService>(),
            Mock.Of<IRedisCacheService>(), Mock.Of<IMapper>());

        var result = await sut.Handle(new GetAllProductsQuery(0, 0), CT);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.INVALID_PAGINATION_PARAMETERS);
    }

    [Fact]
    public async Task GetAllProducts_ShouldReturnCached_WhenCacheHit()
    {
        var cache = new Mock<IRedisCacheService>();
        cache.Setup(x => x.GetDataAsync<PaginatedResult<ProductResponseDtoForClient>>(
                It.IsAny<string>(), CacheConstants.Products))
            .ReturnsAsync(PaginatedResult<ProductResponseDtoForClient>.Success(
                new List<ProductResponseDtoForClient> { new ProductResponseDtoForClient() }, 1, 1, 10));

        var sut = new GetAllProductsHandler(
            ResponseHandler, Mock.Of<IUnitOfWork>(), Mock.Of<IImageUploaderService>(),
            cache.Object, Mock.Of<IMapper>());

        var result = await sut.Handle(new GetAllProductsQuery(1, 10), CT);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    #endregion

    #region GetDetailsOfProductForUser

    [Fact]
    public async Task GetProductDetails_ShouldReturnBadRequest_WhenIdEmpty()
    {
        var sut = new GetDetailsOfProductForUserHandler(
            ResponseHandler, Mock.Of<IUnitOfWork>(), Mock.Of<IImageUploaderService>(),
            Mock.Of<IRedisCacheService>(), Mock.Of<IMapper>());

        var result = await sut.Handle(new GetDetailsOfProductForUserQuery(Guid.Empty), CT);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.INVALID_INPUT);
    }

    [Fact]
    public async Task GetProductDetails_ShouldReturnCached_WhenCacheHit()
    {
        var cache = new Mock<IRedisCacheService>();
        cache.Setup(x => x.GetDataAsync<ProductResponseDtoForClient>(
                It.IsAny<string>(), CacheConstants.Products))
            .ReturnsAsync(new ProductResponseDtoForClient { NameEn = "Cached" });

        var sut = new GetDetailsOfProductForUserHandler(
            ResponseHandler, Mock.Of<IUnitOfWork>(), Mock.Of<IImageUploaderService>(),
            cache.Object, Mock.Of<IMapper>());

        var result = await sut.Handle(new GetDetailsOfProductForUserQuery(Guid.NewGuid()), CT);

        result.Succeeded.Should().BeTrue();
        result.Data.NameEn.Should().Be("Cached");
    }

    [Fact]
    public async Task GetProductDetails_ShouldReturnFailed_WhenProductNotFound()
    {
        var cache = new Mock<IRedisCacheService>();
        cache.Setup(x => x.GetDataAsync<ProductResponseDtoForClient>(
                It.IsAny<string>(), CacheConstants.Products))
            .ReturnsAsync((ProductResponseDtoForClient?)null);

        var products = new Mock<IProductRepository>();
        products.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), false)).ReturnsAsync((SmartCare.Domain.Entities.Product?)null);

        var uow = new UnitOfWorkMockBuilder().WithProducts(products.Object).Build();

        var sut = new GetDetailsOfProductForUserHandler(
            ResponseHandler, uow, Mock.Of<IImageUploaderService>(),
            cache.Object, Mock.Of<IMapper>());

        var result = await sut.Handle(new GetDetailsOfProductForUserQuery(Guid.NewGuid()), CT);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.NOT_FOUND);
    }

    #endregion

    #region FilterProducts

    [Fact]
    public async Task FilterProducts_ShouldReturnBadRequest_WhenPaginationInvalid()
    {
        var sut = new FilterProductsHandler(
            ResponseHandler, Mock.Of<IUnitOfWork>(), Mock.Of<IImageUploaderService>(),
            Mock.Of<IRedisCacheService>(), Mock.Of<IMapper>());

        var result = await sut.Handle(new FilterProductsQuery(new FilterProductsDTo(), 0, 0), CT);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.INVALID_PAGINATION_PARAMETERS);
    }

    #endregion
}
