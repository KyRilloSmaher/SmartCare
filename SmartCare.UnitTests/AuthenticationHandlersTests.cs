using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using SmartCare.Application.CQRs.Authentication.Commands.Auth;
using SmartCare.Application.CQRs.Authentication.Handlers.Auth;
using SmartCare.Application.DTOs.Auth.Requests;
using SmartCare.Application.Features.Authentication.Commands.Passwords.ResetPassword;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using SmartCare.UnitTests.TestHelpers;

namespace SmartCare.UnitTests.Features;

public class AuthenticationHandlersTests
{
    [Fact]
    public async Task Logout_ShouldFail_WhenUserNotFound()
    {
        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(x => x.FindByIdAsync("u1")).ReturnsAsync((ApplictionUser?)null);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.UserManager).Returns(userManager.Object);

        var sut = new LogoutHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<IMapper>());
        var result = await sut.Handle(new LogoutAsyncCommand("u1"), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.USER_NOT_FOUND);
    }

    [Fact]
    public async Task Logout_ShouldSucceed_WhenUserUpdated()
    {
        var user = new ApplictionUser { Id = "u2", RefreshToken = "r" };
        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(x => x.FindByIdAsync("u2")).ReturnsAsync(user);
        userManager.Setup(x => x.UpdateSecurityStampAsync(user)).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.UserManager).Returns(userManager.Object);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var sut = new LogoutHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<IMapper>());
        var result = await sut.Handle(new LogoutAsyncCommand("u2"), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Message.Should().Be(SystemMessages.LOGOUT_SUCCESS);
        user.RefreshToken.Should().BeNull();
    }

    [Fact]
    public async Task ResetPassword_ShouldFail_WhenUserNotFound()
    {
        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(x => x.FindByEmailAsync("x@test.com")).ReturnsAsync((ApplictionUser?)null);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.UserManager).Returns(userManager.Object);

        var sut = new ResetPasswordCommandHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<ILogger<ResetPasswordCommandHandler>>());

        var result = await sut.Handle(new ResetPasswordCommand(new SetNewPasswordRequestDto { Email = "x@test.com", NewPassword = "Password@123456" }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.USER_NOT_FOUND);
    }

    [Fact]
    public async Task ResetPassword_ShouldFail_WhenResetNotConfirmed()
    {
        var user = new ApplictionUser { Id = "u4", Email = "x@test.com", ResetPasswordConfirmed = false };
        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(x => x.FindByEmailAsync("x@test.com")).ReturnsAsync(user);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.UserManager).Returns(userManager.Object);

        var sut = new ResetPasswordCommandHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<ILogger<ResetPasswordCommandHandler>>());

        var result = await sut.Handle(new ResetPasswordCommand(new SetNewPasswordRequestDto { Email = "x@test.com", NewPassword = "Password@123456" }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.RESET_NOT_CONFIRMED);
    }

    [Fact]
    public async Task ResetPassword_ShouldSucceed_WhenFlowValid()
    {
        var user = new ApplictionUser { Id = "u3", Email = "x@test.com", ResetPasswordConfirmed = true };
        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(x => x.FindByEmailAsync("x@test.com")).ReturnsAsync(user);
        userManager.Setup(x => x.RemovePasswordAsync(user)).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(x => x.AddPasswordAsync(user, "Password@123456")).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(x => x.UpdateSecurityStampAsync(user)).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.UserManager).Returns(userManager.Object);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var sut = new ResetPasswordCommandHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<ILogger<ResetPasswordCommandHandler>>());

        var result = await sut.Handle(new ResetPasswordCommand(new SetNewPasswordRequestDto { Email = "x@test.com", NewPassword = "Password@123456" }), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Message.Should().Be(SystemMessages.PASSWORD_RESET_SUCCESS);
        user.ResetPasswordConfirmed.Should().BeFalse();
    }
}
