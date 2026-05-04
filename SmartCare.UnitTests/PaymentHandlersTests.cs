using AutoMapper;
using Moq;
using SmartCare.Application.CQRs.Payments.Queries.GetPaymentByIdQuery;
using SmartCare.Application.CQRs.Payments.Queries.GetPaymentsForOrderIdQuery;
using SmartCare.Application.DTOs.Payment;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;

namespace SmartCare.UnitTests.Features;

public class PaymentHandlersTests
{
    [Fact]
    public async Task GetPaymentById_ShouldReturnMappedResponse()
    {
        var paymentId = Guid.NewGuid();
        var paymentsRepo = new Mock<IPaymentRepository>();
        paymentsRepo.Setup(x => x.GetByIdAsync(paymentId, false))
            .ReturnsAsync(new Payment(Guid.NewGuid(), 100m, SmartCare.Domain.Enums.PaymentMethod.Stripe, "ref-1", "token-1"));

        var mapper = new Mock<IMapper>();
        mapper.Setup(x => x.Map<PaymentResponseDTO>(It.IsAny<Payment>())).Returns(new PaymentResponseDTO { Id = paymentId });

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Payments).Returns(paymentsRepo.Object);

        var sut = new GetPaymentByIdHandler(uow.Object, mapper.Object, new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler());

        var result = await sut.Handle(new GetPaymentByIdQuery(paymentId), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data.Id.Should().Be(paymentId);
    }

    [Fact]
    public async Task GetPaymentsByOrderId_ShouldReturnMappedCollection()
    {
        var orderId = Guid.NewGuid();
        var paymentsRepo = new Mock<IPaymentRepository>();
        paymentsRepo.Setup(x => x.GetPendingPaymentByOrderIdAsync(orderId, false))
            .ReturnsAsync(new Payment(Guid.NewGuid(), 120m, SmartCare.Domain.Enums.PaymentMethod.Stripe, "ref-2", "token-2"));

        var mapper = new Mock<IMapper>();
        mapper.Setup(x => x.Map<IEnumerable<PaymentResponseDTO>>(It.IsAny<object>()))
            .Returns([new PaymentResponseDTO { Id = Guid.NewGuid() }]);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Payments).Returns(paymentsRepo.Object);

        var sut = new GetPaymentsByOrderIdQueryHandler(uow.Object, mapper.Object, new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler());

        var result = await sut.Handle(new GetPaymentsByOrderIdQuery(orderId), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }
}
