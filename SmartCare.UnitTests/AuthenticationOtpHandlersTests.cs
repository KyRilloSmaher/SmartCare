using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using SmartCare.Application.DTOs.Auth.Requests;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Features.Authentication.Commands.Passwords.ComfirmOTPForResetPassword;
using SmartCare.Application.Features.Authentication.Commands.Passwords.ResendOTPForResetPassword;
using SmartCare.Application.Features.Authentication.Commands.Passwords.SendResetPasswordCode;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using SmartCare.UnitTests.TestHelpers;

namespace SmartCare.UnitTests.Features;

public class AuthenticationOtpHandlersTests
{
    [Fact]
    public async Task SendResetCode_ShouldFail_WhenUserNotFound()
    {
        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(x => x.FindByEmailAsync("u@test.com")).ReturnsAsync((ApplictionUser?)null);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.UserManager).Returns(userManager.Object);

        var sut = new SendResetPasswordCodeCommandHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<IEmailService>(), Mock.Of<ILogger<SendResetPasswordCodeCommandHandler>>());
        var result = await sut.Handle(new SendResetPasswordCodeCommand(new ForgetPasswordRequestDto { Email = "u@test.com" }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.USER_NOT_FOUND);
    }

    [Fact]
    public async Task ConfirmResetCode_ShouldFail_WhenNoOtp()
    {
        var user = new ApplictionUser { Email = "u@test.com", OTP = null };
        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(x => x.FindByEmailAsync("u@test.com")).ReturnsAsync(user);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.UserManager).Returns(userManager.Object);

        var sut = new ConfirmResetPasswordCommandHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<ILogger<ConfirmResetPasswordCommandHandler>>());
        var result = await sut.Handle(new ConfirmResetPasswordOTPCommand(new ConfirmResetPasswordCodeRequestDto { Email = "u@test.com", Code = "111111" }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.NO_RESET_CODE);
    }

    [Fact]
    public async Task ConfirmResetCode_ShouldFail_WhenExpired()
    {
        var user = new ApplictionUser { Email = "u@test.com", OTP = BCrypt.Net.BCrypt.HashPassword("111111"), OTPExpiryTime = DateTime.UtcNow.AddMinutes(-1), OTPAttempts = 2 };
        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(x => x.FindByEmailAsync("u@test.com")).ReturnsAsync(user);
        userManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.UserManager).Returns(userManager.Object);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var sut = new ConfirmResetPasswordCommandHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<ILogger<ConfirmResetPasswordCommandHandler>>());
        var result = await sut.Handle(new ConfirmResetPasswordOTPCommand(new ConfirmResetPasswordCodeRequestDto { Email = "u@test.com", Code = "111111" }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.RESET_CODE_EXPIRED);
    }

    [Fact]
    public async Task ConfirmResetCode_ShouldFail_WhenWrongCode()
    {
        var user = new ApplictionUser { Email = "u@test.com", OTP = BCrypt.Net.BCrypt.HashPassword("111111"), OTPExpiryTime = DateTime.UtcNow.AddMinutes(10), OTPAttempts = 0 };
        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(x => x.FindByEmailAsync("u@test.com")).ReturnsAsync(user);
        userManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.UserManager).Returns(userManager.Object);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var sut = new ConfirmResetPasswordCommandHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<ILogger<ConfirmResetPasswordCommandHandler>>());
        var result = await sut.Handle(new ConfirmResetPasswordOTPCommand(new ConfirmResetPasswordCodeRequestDto { Email = "u@test.com", Code = "222222" }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.INVALID_RESET_CODE);
        user.OTPAttempts.Should().Be(1);
    }

    [Fact]
    public async Task ResendResetCode_ShouldFail_WhenRateLimited()
    {
        var user = new ApplictionUser { Email = "u@test.com", OTPExpiryTime = DateTime.UtcNow.AddMinutes(10) };
        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(x => x.FindByEmailAsync("u@test.com")).ReturnsAsync(user);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.UserManager).Returns(userManager.Object);

        var sut = new ReSendOTPForResetPasswordCommandHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<IEmailService>(), Mock.Of<ILogger<ReSendOTPForResetPasswordCommandHandler>>());
        var result = await sut.Handle(new ReSendOTPForResetPasswordCommand(new ForgetPasswordRequestDto { Email = "u@test.com" }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Contain("Please wait");
    }

    [Fact]
    public async Task ConfirmResetCode_ShouldFail_WhenMaxAttemptsReached()
    {
        var user = new ApplictionUser { Email = "u@test.com", OTP = BCrypt.Net.BCrypt.HashPassword("111111"), OTPExpiryTime = DateTime.UtcNow.AddMinutes(10), OTPAttempts = 5 };
        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(x => x.FindByEmailAsync("u@test.com")).ReturnsAsync(user);
        userManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.UserManager).Returns(userManager.Object);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var sut = new ConfirmResetPasswordCommandHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<ILogger<ConfirmResetPasswordCommandHandler>>());
        var result = await sut.Handle(new ConfirmResetPasswordOTPCommand(new ConfirmResetPasswordCodeRequestDto { Email = "u@test.com", Code = "111111" }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.MAX_ATTEMPTS_REACHED);
    }

    [Fact]
    public async Task ConfirmResetCode_ShouldSucceed_WhenValidCode()
    {
        var user = new ApplictionUser { Email = "u@test.com", OTP = BCrypt.Net.BCrypt.HashPassword("111111"), OTPExpiryTime = DateTime.UtcNow.AddMinutes(10), OTPAttempts = 0 };
        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(x => x.FindByEmailAsync("u@test.com")).ReturnsAsync(user);
        userManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.UserManager).Returns(userManager.Object);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var sut = new ConfirmResetPasswordCommandHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<ILogger<ConfirmResetPasswordCommandHandler>>());
        var result = await sut.Handle(new ConfirmResetPasswordOTPCommand(new ConfirmResetPasswordCodeRequestDto { Email = "u@test.com", Code = "111111" }), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Message.Should().Be(SystemMessages.PASSWORD_RESET_CODE_CONFIRMED);
        user.ResetPasswordConfirmed.Should().BeTrue();
    }
}
