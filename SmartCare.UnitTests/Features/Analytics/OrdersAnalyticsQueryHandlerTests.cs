using SmartCare.Application.Features.Analytics.Orders.GetOrdersAnalytics;
using SmartCare.Domain.Projection_Models;

namespace SmartCare.UnitTests.Features.Analytics;

public class OrdersAnalyticsQueryHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenIntervalIsInvalid()
    {
        var uow = new UnitOfWorkMockBuilder().Build();

        var sut = new GetOrdersAnalyticsQueryHandler(
            uow, ResponseHandler, Mock.Of<ILogger<GetOrdersAnalyticsQueryHandler>>());

        var query = new GetOrdersAnalyticsQuery { Interval = "yearly" };
        var result = await sut.Handle(query, CT);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Contain("Invalid interval");
    }

    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenStartDateAfterEndDate()
    {
        var uow = new UnitOfWorkMockBuilder().Build();

        var sut = new GetOrdersAnalyticsQueryHandler(
            uow, ResponseHandler, Mock.Of<ILogger<GetOrdersAnalyticsQueryHandler>>());

        var query = new GetOrdersAnalyticsQuery
        {
            Interval = "daily",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(-7)
        };

        var result = await sut.Handle(query, CT);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Contain("Start date cannot be after end date");
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenNoDataExists()
    {
        var sales = new Mock<ISalesRepository>();
        sales.Setup(x => x.GetOrdersTrendAsync(
                It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new List<OrderTrendItemDto>());

        var uow = new UnitOfWorkMockBuilder().WithSales(sales.Object).Build();

        var sut = new GetOrdersAnalyticsQueryHandler(
            uow, ResponseHandler, Mock.Of<ILogger<GetOrdersAnalyticsQueryHandler>>());

        var query = new GetOrdersAnalyticsQuery { Interval = "daily" };
        var result = await sut.Handle(query, CT);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Contain("No orders data found");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenDataExists()
    {
        var trendData = new List<OrderTrendItemDto>
        {
            new OrderTrendItemDto { Date = "2026-01-01", Orders = 10 },
            new OrderTrendItemDto { Date = "2026-01-02", Orders = 15 }
        };

        var sales = new Mock<ISalesRepository>();
        sales.Setup(x => x.GetOrdersTrendAsync(
                It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(trendData);

        var uow = new UnitOfWorkMockBuilder().WithSales(sales.Object).Build();

        var sut = new GetOrdersAnalyticsQueryHandler(
            uow, ResponseHandler, Mock.Of<ILogger<GetOrdersAnalyticsQueryHandler>>());

        var query = new GetOrdersAnalyticsQuery { Interval = "daily" };
        var result = await sut.Handle(query, CT);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Data.Should().HaveCount(2);
        result.Data.Interval.Should().Be("daily");
    }

    [Fact]
    public async Task Handle_ShouldDefaultToDailyInterval_WhenIntervalEmpty()
    {
        var trendData = new List<OrderTrendItemDto>
        {
            new OrderTrendItemDto { Date = "2026-01-01", Orders = 5 }
        };

        var sales = new Mock<ISalesRepository>();
        sales.Setup(x => x.GetOrdersTrendAsync(
                It.IsAny<Guid?>(), "daily", It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(trendData);

        var uow = new UnitOfWorkMockBuilder().WithSales(sales.Object).Build();

        var sut = new GetOrdersAnalyticsQueryHandler(
            uow, ResponseHandler, Mock.Of<ILogger<GetOrdersAnalyticsQueryHandler>>());

        var query = new GetOrdersAnalyticsQuery { Interval = "" };
        var result = await sut.Handle(query, CT);

        result.Succeeded.Should().BeTrue();
        result.Data.Interval.Should().Be("daily");
    }
}
