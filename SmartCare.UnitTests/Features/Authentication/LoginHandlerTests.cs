using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Moq;
using SmartCare.Application.CQRs.Authentication.Commands.Auth;
using SmartCare.Application.CQRs.Authentication.Handlers.Auth;
using SmartCare.Application.DTOs.Auth.Requests;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Helpers;
using SmartCare.Domain.Interfaces.IServices;
using SmartCare.Domain.IRepositories;
using SmartCare.UnitTests.TestInfrastructure;
using System.Security.Claims;

namespace SmartCare.UnitTests.Features;

public class LoginHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnUnauthorized_WhenUserMissing()
    {
        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(x => x.FindByEmailAsync("u@test.com")).ReturnsAsync((ApplictionUser?)null);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.UserManager).Returns(userManager.Object);

        var tokenService = new Mock<ITokenService>();
        var sut = new LoginHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, tokenService.Object, new JwtSettings { AccessTokenLifetimeHours = 1 }, Mock.Of<IMapper>());

        var result = await sut.Handle(new LoginAsyncCommand(new LoginRequestDto { Email = "u@test.com", Password = "x" }), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        result.Message.Should().Be(SystemMessages.INVALID_CREDENTIALS);
    }

    [Fact]
    public async Task Handle_ShouldReturnUnauthorized_WhenEmailNotConfirmed()
    {
        var user = new ApplictionUser { Id = "u1", Email = "u@test.com", EmailConfirmed = false };
        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(x => x.FindByEmailAsync("u@test.com")).ReturnsAsync(user);
        userManager.Setup(x => x.CheckPasswordAsync(user, "pass")).ReturnsAsync(true);
        userManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string>());

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.UserManager).Returns(userManager.Object);

        var sut = new LoginHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<ITokenService>(), new JwtSettings { AccessTokenLifetimeHours = 1 }, Mock.Of<IMapper>());

        var result = await sut.Handle(new LoginAsyncCommand(new LoginRequestDto { Email = "u@test.com", Password = "pass" }), CancellationToken.None);

        result.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        result.Message.Should().Be(SystemMessages.EMAIL_NOT_CONFIRMED);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        var user = new ApplictionUser { Id = "u2", Email = "u@test.com", EmailConfirmed = true };
        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(x => x.FindByEmailAsync("u@test.com")).ReturnsAsync(user);
        userManager.Setup(x => x.CheckPasswordAsync(user, "pass")).ReturnsAsync(true);
        userManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string>());
        userManager.Setup(x => x.IsLockedOutAsync(user)).ReturnsAsync(false);
        userManager.Setup(x => x.ResetAccessFailedCountAsync(user)).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var tokenService = new Mock<ITokenService>();
        tokenService.Setup(x => x.GetClaimsAsync(user)).ReturnsAsync(new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "u2") });
        tokenService.Setup(x => x.GenerateAccessToken(It.IsAny<IEnumerable<Claim>>())).Returns("access");
        tokenService.Setup(x => x.GenerateRefreshToken()).Returns("refresh");
        tokenService.Setup(x => x.GetRefreshTokenExpiryTime()).Returns(DateTime.UtcNow.AddDays(7));

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.UserManager).Returns(userManager.Object);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var sut = new LoginHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, tokenService.Object, new JwtSettings { AccessTokenLifetimeHours = 1 }, Mock.Of<IMapper>());

        var result = await sut.Handle(new LoginAsyncCommand(new LoginRequestDto { Email = "u@test.com", Password = "pass" }), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Message.Should().Be(SystemMessages.LOGIN_SUCCESS);
        result.Data.AccessToken.Should().Be("access");
        result.Data.RefreshToken.Should().Be("refresh");
    }
}
