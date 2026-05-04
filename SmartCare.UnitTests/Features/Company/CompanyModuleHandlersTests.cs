using AutoMapper;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Logging;
using Moq;
using SmartCare.Application.Companies.Requests;
using SmartCare.Application.DTOs.Companies.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Features.Company.Commands;
using SmartCare.Application.Features.Company.Commands.ChangeLogo;
using SmartCare.Application.Features.Company.Commands.Delete;
using SmartCare.Application.Features.Company.Commands.Restore;
using SmartCare.Application.Features.Company.Commands.RestoreCompany;
using SmartCare.Application.Features.Company.Commands.Update;
using SmartCare.Application.Features.Company.Handlers;
using SmartCare.Application.Features.Company.Queries.GetAll;
using SmartCare.Application.Features.Company.Queries.GetAllByPaginated;
using SmartCare.Application.Features.Company.Queries.GetAllForAdmin;
using SmartCare.Application.Features.Company.Queries.SearchByName;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;

namespace SmartCare.UnitTests.Features;

public class CompanyModuleHandlersTests
{
    [Fact]
    public async Task DeleteCompany_ShouldReturnBadRequest_WhenIdEmpty()
    {
        var sut = new DeleteCompanyCommandHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<IUnitOfWork>(), Mock.Of<IRedisCacheService>(), Mock.Of<IImageUploaderService>(), Mock.Of<ILogger<DeleteCompanyCommandHandler>>());
        var result = await sut.Handle(new DeleteCompanyCommand(Guid.Empty), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.INVALID_INPUT);
    }

    [Fact]
    public async Task UpdateCompany_ShouldReturnFailed_WhenNotFound()
    {
        var companies = new Mock<ICompanyRepository>();
        companies.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), true)).ReturnsAsync((Company?)null);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Companies).Returns(companies.Object);

        var sut = new UpdateCompanyCommandHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<IRedisCacheService>(), Mock.Of<IImageUploaderService>(), Mock.Of<IMapper>(), Mock.Of<ILogger<UpdateCompanyCommandHandler>>());
        var result = await sut.Handle(new UpdateCompanyCommand(new UpdateCompanyRequest { Id = Guid.NewGuid(), Name = "x" }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.NOT_FOUND);
    }

    [Fact]
    public async Task RestoreCompany_ShouldReturnAlreadyActive_WhenNotDeleted()
    {
        var company = new Company { Id = Guid.NewGuid(), IsDeleted = false };
        var companies = new Mock<ICompanyRepository>();
        companies.Setup(x => x.GetByIdAsync(company.Id, true)).ReturnsAsync(company);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Companies).Returns(companies.Object);

        var sut = new RestoreCompanyCommandHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<IRedisCacheService>(), Mock.Of<ILogger<RestoreCompanyCommandHandler>>());
        var result = await sut.Handle(new RestoreCompanyCommand(company.Id), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Message.Should().Be(SystemMessages.ALREADY_ACTIVE);
    }

    [Fact]
    public async Task ChangeCompanyLogo_ShouldFail_WhenUploadFails()
    {
        var company = new Company { Id = Guid.NewGuid(), LogoUrl = "old" };
        var companies = new Mock<ICompanyRepository>();
        companies.Setup(x => x.GetByIdAsync(company.Id, true)).ReturnsAsync(company);

        var uploader = new Mock<IImageUploaderService>();
        uploader.Setup(x => x.UploadImageAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(), ImageFolder.BrandLogos))
            .ReturnsAsync(new ImageUploadResult { Error = new Error { Message = "err" } });

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Companies).Returns(companies.Object);

        var sut = new ChangeCompanyLogoHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<IRedisCacheService>(), uploader.Object, Mock.Of<ILogger<ChangeCompanyLogoHandler>>());
        var result = await sut.Handle(new ChangeCompanyLogoCommand(new ChangeCompanyLogoRequestDto { Id = company.Id, Image = Mock.Of<Microsoft.AspNetCore.Http.IFormFile>() }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.FILE_UPLOAD_FAILED);
    }

    [Fact]
    public async Task GetAllCompanies_ShouldReturnCached_WhenCacheHit()
    {
        var cache = new Mock<IRedisCacheService>();
        cache.Setup(x => x.GetDataAsync<IEnumerable<CompanyResponseDto>>(CacheConstants.CompaniesClient, CacheConstants.Companies))
            .ReturnsAsync(new List<CompanyResponseDto> { new CompanyResponseDto { Id = Guid.NewGuid() } });

        var sut = new GetAllCompaniesQueryHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<IUnitOfWork>(), cache.Object, Mock.Of<IMapper>(), Mock.Of<ILogger<GetAllCompaniesQueryHandler>>());
        var result = await sut.Handle(new GetAllCompaniesQuery(), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllCompaniesPaginated_ShouldReturnBadRequest_WhenInvalidPaging()
    {
        var sut = new GetAllCompaniesPaginatedQueryHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<IUnitOfWork>(), Mock.Of<IRedisCacheService>(), Mock.Of<IMapper>(), Mock.Of<ILogger<GetAllCompaniesPaginatedQueryHandler>>());
        var result = await sut.Handle(new GetAllCompaniesPaginatedQuery(0, 0), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Contain("Page Number");
    }

    [Fact]
    public async Task GetAllCompaniesForAdmin_ShouldReturnCached_WhenCacheHit()
    {
        var cache = new Mock<IRedisCacheService>();
        cache.Setup(x => x.GetDataAsync<IEnumerable<CompanyResponseForAdminDto>>(CacheConstants.CompanyiesAllAdmin, CacheConstants.Companies))
            .ReturnsAsync(new List<CompanyResponseForAdminDto> { new CompanyResponseForAdminDto { Id = Guid.NewGuid() } });

        var sut = new GetAllCompaniesForAdminQueryHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<IUnitOfWork>(), cache.Object, Mock.Of<IMapper>(), Mock.Of<ILogger<GetAllCompaniesForAdminQueryHandler>>());
        var result = await sut.Handle(new GetAllCompaniesForAdminQuery(), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchCompaniesByName_ShouldReturnBadRequest_WhenNameEmpty()
    {
        var sut = new SearchCompaniesByNameQueryHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<IUnitOfWork>(), Mock.Of<IMapper>(), Mock.Of<ILogger<SearchCompaniesByNameQueryHandler>>());
        var result = await sut.Handle(new SearchCompaniesByNameQuery(""), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Contain("Name cannot be empty");
    }
}
