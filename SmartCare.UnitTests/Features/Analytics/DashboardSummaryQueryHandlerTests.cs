using SmartCare.Application.Features.Analytics.DashBoard.Summary;
using SmartCare.Domain.Projection_Models;

namespace SmartCare.UnitTests.Features.Analytics;

public class DashboardSummaryQueryHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenDataReturned()
    {
        var summaryDto = new DashboardSummaryDto
        {
            TotalOrders = 100,
            TotalRevenue = 5000m
        };

        var sales = new Mock<ISalesRepository>();
        sales.Setup(x => x.GetDashboardSummaryAsync(null, null, null))
            .ReturnsAsync(summaryDto);

        var uow = new UnitOfWorkMockBuilder().WithSales(sales.Object).Build();

        var sut = new GetDashboardSummaryQueryHandler(
            uow, ResponseHandler, Mock.Of<ILogger<GetDashboardSummaryQueryHandler>>());

        var result = await sut.Handle(
            new GetDashboardSummaryQuery(null, null, null), CT);

        result.Succeeded.Should().BeTrue();
        result.Data.TotalOrders.Should().Be(100);
        result.Data.TotalRevenue.Should().Be(5000m);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailed_WhenExceptionThrown()
    {
        var sales = new Mock<ISalesRepository>();
        sales.Setup(x => x.GetDashboardSummaryAsync(It.IsAny<Guid?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ThrowsAsync(new Exception("DB error"));

        var uow = new UnitOfWorkMockBuilder().WithSales(sales.Object).Build();

        var sut = new GetDashboardSummaryQueryHandler(
            uow, ResponseHandler, Mock.Of<ILogger<GetDashboardSummaryQueryHandler>>());

        var result = await sut.Handle(
            new GetDashboardSummaryQuery(Guid.NewGuid(), DateTime.UtcNow.AddDays(-7), DateTime.UtcNow), CT);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Contain("Failed to retrieve dashboard summary");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WithBranchFilter()
    {
        var branchId = Guid.NewGuid();
        var summaryDto = new DashboardSummaryDto { TotalOrders = 50, TotalRevenue = 2500m };

        var sales = new Mock<ISalesRepository>();
        sales.Setup(x => x.GetDashboardSummaryAsync(branchId, It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(summaryDto);

        var uow = new UnitOfWorkMockBuilder().WithSales(sales.Object).Build();

        var sut = new GetDashboardSummaryQueryHandler(
            uow, ResponseHandler, Mock.Of<ILogger<GetDashboardSummaryQueryHandler>>());

        var result = await sut.Handle(
            new GetDashboardSummaryQuery(branchId, null, null), CT);

        result.Succeeded.Should().BeTrue();
        result.Data.TotalOrders.Should().Be(50);
    }
}
