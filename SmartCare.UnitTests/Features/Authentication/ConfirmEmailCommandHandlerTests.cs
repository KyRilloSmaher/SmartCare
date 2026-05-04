using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Moq;
using SmartCare.Application.DTOs.Auth.Requests;
using SmartCare.Application.Features.Authentication.Commands.Email.ConfirmEmail;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using SmartCare.UnitTests.TestInfrastructure;
using System.Text;

namespace SmartCare.UnitTests.Features;

public class ConfirmEmailCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldFail_WhenUserNotFound()
    {
        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(x => x.FindByEmailAsync("u@test.com")).ReturnsAsync((ApplictionUser?)null);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.UserManager).Returns(userManager.Object);

        var sut = new ConfirmEmailCommandHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<IMapper>(), Mock.Of<ILogger<ConfirmEmailCommandHandler>>());
        var token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes("abc"));
        var result = await sut.Handle(new ConfirmEmailCommand(new ConfirmEmailRequest { Email = "u@test.com", Token = token }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.USER_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenVerificationMissing()
    {
        var user = new ApplictionUser { Id = "u1", Email = "u@test.com", EmailConfirmed = false };
        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(x => x.FindByEmailAsync("u@test.com")).ReturnsAsync(user);

        var emailVerRepo = new Mock<IEmailVerificationRepository>();
        emailVerRepo.Setup(x => x.GetValidVerificationAsync("u@test.com", "abc")).ReturnsAsync((EmailVerification?)null);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.UserManager).Returns(userManager.Object);
        uow.SetupGet(x => x.EmailVerifications).Returns(emailVerRepo.Object);

        var sut = new ConfirmEmailCommandHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<IMapper>(), Mock.Of<ILogger<ConfirmEmailCommandHandler>>());
        var token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes("abc"));
        var result = await sut.Handle(new ConfirmEmailCommand(new ConfirmEmailRequest { Email = "u@test.com", Token = token }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.INVALID_TOKEN);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenVerificationAndIdentityConfirmationPass()
    {
        var user = new ApplictionUser { Id = "u2", Email = "u@test.com", EmailConfirmed = false };
        var verification = new EmailVerification { Email = "u@test.com", Token = "abc", ExpiresAt = DateTime.UtcNow.AddHours(1) };

        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(x => x.FindByEmailAsync("u@test.com")).ReturnsAsync(user);
        userManager.Setup(x => x.ConfirmEmailAsync(user, "abc")).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "CLIENT" });

        var emailVerRepo = new Mock<IEmailVerificationRepository>();
        emailVerRepo.Setup(x => x.GetValidVerificationAsync("u@test.com", "abc")).ReturnsAsync(verification);

        var carts = new Mock<ICartRepository>();
        carts.Setup(x => x.CreateCartAsync(user.Id)).ReturnsAsync(new Cart { Id = Guid.NewGuid(), ClientId = user.Id });

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.UserManager).Returns(userManager.Object);
        uow.SetupGet(x => x.EmailVerifications).Returns(emailVerRepo.Object);
        uow.SetupGet(x => x.Carts).Returns(carts.Object);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var sut = new ConfirmEmailCommandHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<IMapper>(), Mock.Of<ILogger<ConfirmEmailCommandHandler>>());
        var token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes("abc"));
        var result = await sut.Handle(new ConfirmEmailCommand(new ConfirmEmailRequest { Email = "u@test.com", Token = token }), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Message.Should().Be(SystemMessages.VERIFICATION_SUCCESS);
        user.EmailConfirmed.Should().BeTrue();
    }
}
