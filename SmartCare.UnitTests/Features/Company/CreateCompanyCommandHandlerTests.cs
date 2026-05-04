using AutoMapper;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Logging;
using Moq;
using SmartCare.Application.Companies.Requests;
using SmartCare.Application.DTOs.Companies.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Features.Company.Commands.Create;
using SmartCare.Application.Features.Company.Handlers;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;

namespace SmartCare.UnitTests.Features;

public class CreateCompanyCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCreateSucceeds()
    {
        var companyRepo = new Mock<ICompanyRepository>();
        companyRepo.Setup(r => r.AddAsync(It.IsAny<Company>())).ReturnsAsync((Company c) => c);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Companies).Returns(companyRepo.Object);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var uploader = new Mock<IImageUploaderService>();
        uploader.Setup(x => x.UploadImageAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(), ImageFolder.BrandLogos))
            .ReturnsAsync(new ImageUploadResult { Url = new Uri("https://cdn.test/logo.png") });

        var mapper = new Mock<IMapper>();
        mapper.Setup(x => x.Map<Company>(It.IsAny<CreateCompanyRequestDto>())).Returns(new Company { Name = "Acme" });
        mapper.Setup(x => x.Map<CompanyResponseForAdminDto>(It.IsAny<Company>())).Returns(new CompanyResponseForAdminDto { Name = "Acme" });

        var cache = new Mock<IRedisCacheService>();
        var responseHandler = new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler();
        var logger = Mock.Of<ILogger<CreateCompanyCommandHandler>>();

        var sut = new CreateCompanyCommandHandler(responseHandler, uow.Object, uploader.Object, mapper.Object, cache.Object, logger);
        var cmd = new CreateCompanyCommand(new CreateCompanyRequestDto { Name = "Acme", Logo = Mock.Of<Microsoft.AspNetCore.Http.IFormFile>() });

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Message.Should().Be(SystemMessages.SUCCESS);
        result.Data.Name.Should().Be("Acme");
        cache.Verify(x => x.DeleteKeysByTag(CacheConstants.Companies), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailed_WhenUploadFails()
    {
        var uow = new Mock<IUnitOfWork>();
        var uploader = new Mock<IImageUploaderService>();
        uploader.Setup(x => x.UploadImageAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(), ImageFolder.BrandLogos))
            .ReturnsAsync(new ImageUploadResult { Error = new Error { Message = "upload error" } });

        var mapper = new Mock<IMapper>();
        var cache = new Mock<IRedisCacheService>();
        var responseHandler = new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler();
        var logger = Mock.Of<ILogger<CreateCompanyCommandHandler>>();

        var sut = new CreateCompanyCommandHandler(responseHandler, uow.Object, uploader.Object, mapper.Object, cache.Object, logger);
        var cmd = new CreateCompanyCommand(new CreateCompanyRequestDto { Name = "Acme", Logo = Mock.Of<Microsoft.AspNetCore.Http.IFormFile>() });

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.FILE_UPLOAD_FAILED);
    }
}
