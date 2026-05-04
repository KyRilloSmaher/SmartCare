using AutoMapper;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Identity;
using Moq;
using SmartCare.Application.CQRs.Client.Commands;
using SmartCare.Application.CQRs.Client.Handlers;
using SmartCare.Application.CQRs.Client.Queries;
using SmartCare.Application.DTOs.Client.Requests;
using SmartCare.Application.DTOs.Client.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using SmartCare.UnitTests.TestInfrastructure;

namespace SmartCare.UnitTests.Features;

public class ClientModuleHandlersTests
{
    [Fact]
    public async Task GetClientById_ShouldReturnBadRequest_WhenIdEmpty()
    {
        var sut = new GetClientByIdHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<IBackgroundJobService>(), Mock.Of<IUnitOfWork>(), Mock.Of<IRedisCacheService>(), Mock.Of<IImageUploaderService>(), Mock.Of<IMapper>());
        var result = await sut.Handle(new GetClientByIdAsyncQuery(""), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.USER_NOT_FOUND);
    }

    [Fact]
    public async Task GetClientByEmail_ShouldReturnNotFound_WhenMissing()
    {
        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(x => x.FindByEmailAsync("u@test.com")).ReturnsAsync((ApplictionUser?)null);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.UserManager).Returns(userManager.Object);

        var sut = new GetClientByEmailHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<IRedisCacheService>(), uow.Object, Mock.Of<IMapper>());
        var result = await sut.Handle(new GetClientByEmailAsyncQuery("u@test.com"), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteClient_ShouldReturnBadRequest_WhenIdEmpty()
    {
        var sut = new DeleteClientHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<IBackgroundJobService>(), Mock.Of<IUnitOfWork>(), Mock.Of<IRedisCacheService>(), Mock.Of<IImageUploaderService>(), Mock.Of<IMapper>());
        var result = await sut.Handle(new DeleteClientAsyncCommand(""), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.USER_NOT_FOUND);
    }

    [Fact]
    public async Task ChangeClientImage_ShouldFail_WhenOldImageDeleteFails()
    {
        var user = new ApplictionUser { Id = "u1", ProfileImageUrl = "old-url" };
        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(x => x.FindByIdAsync("u1")).ReturnsAsync(user);

        var uploader = new Mock<IImageUploaderService>();
        uploader.Setup(x => x.DeleteImageByUrlAsync("old-url")).ReturnsAsync(false);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.UserManager).Returns(userManager.Object);

        var sut = new ChangeClientProfileImageHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<IBackgroundJobService>(), Mock.Of<IRedisCacheService>(), uploader.Object, Mock.Of<IMapper>(), uow.Object);
        var result = await sut.Handle(new ChangeClientProfileImageAsyncCommand("u1", new ChangeClientProfileImageRequestDto { ProfileImage = Mock.Of<Microsoft.AspNetCore.Http.IFormFile>() }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.FAILED);
    }

    [Fact]
    public async Task UpdateClient_ShouldReturnNotFound_WhenClientMissing()
    {
        var clients = new Mock<IClientRepository>();
        clients.Setup(x => x.GetByIdAsync("u1", true)).ReturnsAsync((Client?)null);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Clients).Returns(clients.Object);

        var sut = new UpdateClientHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<IBackgroundJobService>(), uow.Object, Mock.Of<IRedisCacheService>(), Mock.Of<IImageUploaderService>(), Mock.Of<IMapper>());
        var result = await sut.Handle(new UpdateClientAsyncCommand("u1", new UpdateClientRequest { UserName = "x" }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.USER_NOT_FOUND);
    }
}
