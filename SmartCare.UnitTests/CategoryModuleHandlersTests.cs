using AutoMapper;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Logging;
using Moq;
using SmartCare.Application.CQRs.Category.Handlers;
using SmartCare.Application.DTOs.Caregory.Requests;
using SmartCare.Application.DTOs.Caregory.Response;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Features.Category.Commands;
using SmartCare.Application.Features.Category.Commands.ChangeCategoryLogo;
using SmartCare.Application.Features.Category.Commands.CreateCategory;
using SmartCare.Application.Features.Category.Commands.RestoreCategory;
using SmartCare.Application.Features.Category.Queries.GetAll;
using SmartCare.Application.Features.Category.Queries.GetAllCategoriesForAdmin;
using SmartCare.Application.Features.Category.Queries.GetAllpaginated;
using SmartCare.Application.Features.Category.Queries.GetCategoryById;
using SmartCare.Application.Features.Category.Queries.SearchForCatgeory;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;

namespace SmartCare.UnitTests.Features;

public class CategoryModuleHandlersTests
{
    [Fact]
    public async Task CreateCategory_ShouldFail_WhenUploadFails()
    {
        var uploader = new Mock<IImageUploaderService>();
        uploader.Setup(x => x.UploadImageAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(), ImageFolder.CategoryImages))
            .ReturnsAsync(new ImageUploadResult { Error = new Error { Message = "err" } });

        var sut = new CreateCategoryCommandHandler(
            new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(),
            Mock.Of<IUnitOfWork>(),
            uploader.Object,
            Mock.Of<IMapper>(),
            Mock.Of<IRedisCacheService>(),
            Mock.Of<ILogger<CreateCategoryCommandHandler>>());

        var result = await sut.Handle(new CreateCategoryCommand(new CreateCategoryRequestDto { Name = "Cat", Logo = Mock.Of<Microsoft.AspNetCore.Http.IFormFile>() }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.FILE_UPLOAD_FAILED);
    }

    [Fact]
    public async Task DeleteCategory_ShouldReturnBadRequest_WhenIdEmpty()
    {
        var sut = new DeleteCategoryCommandHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<IUnitOfWork>(), Mock.Of<IRedisCacheService>(), Mock.Of<IImageUploaderService>(), Mock.Of<ILogger<DeleteCategoryCommandHandler>>());
        var result = await sut.Handle(new DeleteCategoryCommand(Guid.Empty), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.INVALID_INPUT);
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnFailed_WhenNotFound()
    {
        var categories = new Mock<ICategoryRepository>();
        categories.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), true)).ReturnsAsync((Category?)null);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Categories).Returns(categories.Object);

        var sut = new UpdateCategoryCommandHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<IRedisCacheService>(), Mock.Of<IImageUploaderService>(), Mock.Of<IMapper>(), Mock.Of<ILogger<UpdateCategoryCommandHandler>>());
        var result = await sut.Handle(new UpdateCategoryCommand(new UpdateCategoryRequest { Id = Guid.NewGuid(), Name = "x" }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.NOT_FOUND);
    }

    [Fact]
    public async Task RestoreCategory_ShouldReturnAlreadyActive_WhenNotDeleted()
    {
        var cat = new Category { Id = Guid.NewGuid(), IsDeleted = false };
        var categories = new Mock<ICategoryRepository>();
        categories.Setup(x => x.GetByIdAsync(cat.Id, true)).ReturnsAsync(cat);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Categories).Returns(categories.Object);

        var sut = new RestoreCategoryCommandHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<IRedisCacheService>(), Mock.Of<ILogger<RestoreCategoryCommandHandler>>());
        var result = await sut.Handle(new RestoreCategoryCommand(cat.Id), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Message.Should().Be(SystemMessages.ALREADY_ACTIVE);
    }

    [Fact]
    public async Task ChangeCategoryLogo_ShouldReturnFailed_WhenCategoryMissing()
    {
        var categories = new Mock<ICategoryRepository>();
        categories.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), true)).ReturnsAsync((Category?)null);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Categories).Returns(categories.Object);

        var sut = new ChangeCategoryLogoHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<IRedisCacheService>(), Mock.Of<IImageUploaderService>(), Mock.Of<ILogger<ChangeCategoryLogoHandler>>());
        var result = await sut.Handle(new ChangeCategoryLogoCommand(new ChangeCategoryLogoRequestDto { Id = Guid.NewGuid(), Image = Mock.Of<Microsoft.AspNetCore.Http.IFormFile>() }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.NOT_FOUND);
    }

    [Fact]
    public async Task GetAllCategories_ShouldReturnCached_WhenCacheHit()
    {
        var cache = new Mock<IRedisCacheService>();
        cache.Setup(x => x.GetDataAsync<IEnumerable<CategoryResponseDto>>(CacheConstants.CategoriesClient, CacheConstants.Categories))
            .ReturnsAsync(new List<CategoryResponseDto> { new CategoryResponseDto { Id = Guid.NewGuid() } });

        var sut = new GetAllCategoryiesQueryHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<IUnitOfWork>(), cache.Object, Mock.Of<IMapper>(), Mock.Of<ILogger<GetAllCategoryiesQueryHandler>>());
        var result = await sut.Handle(new GetAllCategoryiesQuery(), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetCategoryById_ShouldReturnBadRequest_WhenIdEmpty()
    {
        var sut = new GetCategoryByIdQueryHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<IUnitOfWork>(), Mock.Of<IRedisCacheService>(), Mock.Of<IMapper>(), Mock.Of<ILogger<GetCategoryByIdQueryHandler>>());
        var result = await sut.Handle(new GetCategoryByIdQuery(Guid.Empty), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.INVALID_INPUT);
    }

    [Fact]
    public async Task GetAllCategoriesPaginated_ShouldReturnBadRequest_WhenPagingInvalid()
    {
        var sut = new GetAllCategoriesPaginatedQueryHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<IUnitOfWork>(), Mock.Of<IRedisCacheService>(), Mock.Of<IMapper>(), Mock.Of<ILogger<GetAllCategoriesPaginatedQueryHandler>>());
        var result = await sut.Handle(new GetAllCategoriesPaginatedQuery(0, 0), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Contain("Page Number");
    }

    [Fact]
    public async Task GetAllCategoriesForAdmin_ShouldReturnCached_WhenCacheHit()
    {
        var cache = new Mock<IRedisCacheService>();
        cache.Setup(x => x.GetDataAsync<IEnumerable<CategoryResponseForAdminDto>>(CacheConstants.CategoriesAllAdmin, CacheConstants.Categories))
            .ReturnsAsync(new List<CategoryResponseForAdminDto> { new CategoryResponseForAdminDto { Id = Guid.NewGuid() } });

        var sut = new GetAllCategorysForAdminQueryHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<IUnitOfWork>(), cache.Object, Mock.Of<IMapper>(), Mock.Of<ILogger<GetAllCategorysForAdminQueryHandler>>());
        var result = await sut.Handle(new GetAllCategoriesForAdminQuery(), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchCategoriesByName_ShouldReturnMappedResult()
    {
        var categories = new List<Category> { new Category { Id = Guid.NewGuid(), Name = "Pain" } };

        var repo = new Mock<ICategoryRepository>();
        repo.Setup(x => x.SearchCategoryByNameAsync("pain")).ReturnsAsync(categories);

        var mapper = new Mock<IMapper>();
        mapper.Setup(x => x.Map<IEnumerable<CategoryResponseDto>>(categories)).Returns(new List<CategoryResponseDto> { new CategoryResponseDto { Name = "Pain" } });

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Categories).Returns(repo.Object);

        var sut = new SearchCategoriesByNameQueryHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), uow.Object, Mock.Of<IRedisCacheService>(), Mock.Of<IImageUploaderService>(), mapper.Object);
        var result = await sut.Handle(new SearchCategoriesByNameQuery("pain"), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }
}
