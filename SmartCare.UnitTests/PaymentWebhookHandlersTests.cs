using Microsoft.Extensions.Logging;
using Moq;
using SmartCare.Application.CQRs.Payment.Extensions;
using SmartCare.Application.CQRs.Payments.Commands.HandlePaymentFailedCommand;
using SmartCare.Application.CQRs.Payments.Commands.HandlePaymentSucceededCommand;
using SmartCare.Application.DTOs.Payment;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.IServices;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;

namespace SmartCare.UnitTests.Features;

public class PaymentWebhookHandlersTests
{
    private static PaymentExtensions BuildPaymentExtensions(IUnitOfWork uow)
    {
        return new PaymentExtensions(uow, Mock.Of<IBackgroundJobService>(), Mock.Of<IEmailService>());
    }

    [Fact]
    public async Task HandlePaymentFailed_ShouldReturnFailed_WhenOrderMissing()
    {
        var orders = new Mock<IOrderRepository>();
        orders.Setup(x => x.GetOrderWithDetailsByIdAsync(It.IsAny<Guid>(), false)).ReturnsAsync((Order?)null);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Orders).Returns(orders.Object);

        var sut = new HandlePaymentIntentFailedCommandHandler(
            uow.Object,
            Mock.Of<ILogger<HandlePaymentIntentFailedCommandHandler>>(),
            BuildPaymentExtensions(uow.Object),
            new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(),
            Mock.Of<IBackgroundJobService>());

        var cmd = new HandlePaymentIntentFailedAsyncCommand(new PaymentWebhookResult { OrderId = Guid.NewGuid() });
        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandlePaymentFailed_ShouldMarkPaymentFailed_WhenPendingOrder()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, Status = OrderStatus.Pending, ClientId = "c1" };
        var payment = new Payment(orderId, 30m, PaymentMethod.Stripe, "ref-1", "token-1");

        var orders = new Mock<IOrderRepository>();
        orders.Setup(x => x.GetOrderWithDetailsByIdAsync(orderId, false)).ReturnsAsync(order);

        var payments = new Mock<IPaymentRepository>();
        payments.Setup(x => x.GetPendingPaymentByOrderIdAsync(orderId, true)).ReturnsAsync(payment);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Orders).Returns(orders.Object);
        uow.SetupGet(x => x.Payments).Returns(payments.Object);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var sut = new HandlePaymentIntentFailedCommandHandler(
            uow.Object,
            Mock.Of<ILogger<HandlePaymentIntentFailedCommandHandler>>(),
            BuildPaymentExtensions(uow.Object),
            new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(),
            Mock.Of<IBackgroundJobService>());

        var cmd = new HandlePaymentIntentFailedAsyncCommand(new PaymentWebhookResult { OrderId = orderId });
        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.PaymentFailed);
        payment.Status.Should().Be(PaymentStatus.Failed);
    }

    [Fact]
    public async Task HandlePaymentSucceeded_ShouldReturnFailed_WhenOrderMissing()
    {
        var orderId = Guid.NewGuid();
        var orders = new Mock<IOrderRepository>();
        orders.Setup(x => x.GetOrderWithDetailsByIdAsync(orderId, true)).ReturnsAsync((Order?)null);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Orders).Returns(orders.Object);

        var sut = new HandlePaymentIntentSucceededHandler(
            uow.Object,
            Mock.Of<ILogger<HandlePaymentIntentSucceededHandler>>(),
            BuildPaymentExtensions(uow.Object),
            new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(),
            Mock.Of<IBackgroundJobService>(),
            Mock.Of<IEventPublisherService>());

        var result = await sut.Handle(new HandlePaymentSucceededAsyncCommand(new PaymentWebhookResult { OrderId = orderId, ProviderReferenceId = "ref", Amount = 10m }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandlePaymentSucceeded_ShouldReturnFailed_WhenPaymentReferenceMismatch()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, Status = OrderStatus.Pending, TotalPrice = 10m, ClientId = "c1" };

        var orders = new Mock<IOrderRepository>();
        orders.Setup(x => x.GetOrderWithDetailsByIdAsync(orderId, true)).ReturnsAsync(order);

        var payments = new Mock<IPaymentRepository>();
        payments.Setup(x => x.GetPendingPaymentByOrderIdAsync(orderId, true))
            .ReturnsAsync(new Payment(orderId, 10m, PaymentMethod.Stripe, "different-ref", "token"));

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Orders).Returns(orders.Object);
        uow.SetupGet(x => x.Payments).Returns(payments.Object);

        var sut = new HandlePaymentIntentSucceededHandler(
            uow.Object,
            Mock.Of<ILogger<HandlePaymentIntentSucceededHandler>>(),
            BuildPaymentExtensions(uow.Object),
            new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(),
            Mock.Of<IBackgroundJobService>(),
            Mock.Of<IEventPublisherService>());

        var result = await sut.Handle(new HandlePaymentSucceededAsyncCommand(new PaymentWebhookResult { OrderId = orderId, ProviderReferenceId = "ref-1", Amount = 10m }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
    }
}
