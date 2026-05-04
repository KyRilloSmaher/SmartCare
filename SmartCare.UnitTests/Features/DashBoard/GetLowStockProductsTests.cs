using SmartCare.Application.Features.DashBoard.Queries.GetLowStockProducts;

namespace SmartCare.UnitTests.Features.DashBoard;

public class GetLowStockProductsTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenPaginationInvalid()
    {
        var sut = new GetLowStockProductsQueryHandler(Mock.Of<IUnitOfWork>(), ResponseHandler);

        var query = new GetLowStockProductsQuery { PageNumber = 0, PageSize = 0 };
        var result = await sut.Handle(query, CT);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.INVALID_PAGINATION_PARAMETERS);
    }

    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenNegativePageSize()
    {
        var sut = new GetLowStockProductsQueryHandler(Mock.Of<IUnitOfWork>(), ResponseHandler);

        var query = new GetLowStockProductsQuery { PageNumber = 1, PageSize = -5 };
        var result = await sut.Handle(query, CT);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.INVALID_PAGINATION_PARAMETERS);
    }
}
