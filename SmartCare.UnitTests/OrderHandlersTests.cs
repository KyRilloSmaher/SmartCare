using AutoMapper;
using Moq;
using SmartCare.Application.CQRs.Order.Handlers;
using SmartCare.Application.CQRs.Order.Queries;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;

namespace SmartCare.UnitTests.Features;

public class OrderHandlersTests
{
    [Fact]
    public async Task GetOrderById_ShouldReturnBadRequest_WhenIdEmpty()
    {
        var uow = new Mock<IUnitOfWork>();
        var sut = new GetOrderByIdHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<IMapper>());

        var result = await sut.Handle(new GetOrderByIdAsyncQuery(Guid.Empty), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.BAD_REQUEST);
    }

    [Fact]
    public async Task GetTotalOrdersCount_ShouldReturnCount()
    {
        var orders = new Mock<IOrderRepository>();
        orders.Setup(x => x.GetTotalOrdersCountAsync(null)).ReturnsAsync(7);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Orders).Returns(orders.Object);

        var sut = new GetTotalOrdersCountHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object);

        var result = await sut.Handle(new GetTotalOrdersCountAsyncQuery(), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().Be(7);
    }

    [Fact]
    public async Task GetTotalRevenue_ShouldReturnRevenue()
    {
        var orders = new Mock<IOrderRepository>();
        orders.Setup(x => x.GetTotalRevenueAsync(null)).ReturnsAsync(123.45m);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Orders).Returns(orders.Object);

        var sut = new GetTotalRevenueHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object);

        var result = await sut.Handle(new GetTotalRevenueAsyncQuery(), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().Be(123.45m);
    }

    [Fact]
    public async Task GetOrderById_ShouldReturnNotFound_WhenMissing()
    {
        var orders = new Mock<IOrderRepository>();
        orders.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), false)).ReturnsAsync((Order?)null);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Orders).Returns(orders.Object);

        var sut = new GetOrderByIdHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<IMapper>());

        var result = await sut.Handle(new GetOrderByIdAsyncQuery(Guid.NewGuid()), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.ORDER_NOT_FOUND);
    }

    [Fact]
    public async Task GetOrderById_ShouldReturnSuccess_WhenFound()
    {
        var orderId = Guid.NewGuid();
        var orders = new Mock<IOrderRepository>();
        orders.Setup(x => x.GetByIdAsync(orderId, false)).ReturnsAsync(new Order { Id = orderId });

        var mapper = new Mock<IMapper>();
        mapper.Setup(x => x.Map<OrderResponseDto>(It.IsAny<Order>())).Returns(new OrderResponseDto { Id = orderId });

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Orders).Returns(orders.Object);

        var sut = new GetOrderByIdHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, mapper.Object);

        var result = await sut.Handle(new GetOrderByIdAsyncQuery(orderId), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data.Id.Should().Be(orderId);
    }
}
