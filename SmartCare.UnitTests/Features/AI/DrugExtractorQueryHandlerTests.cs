using SmartCare.Application.ExternalServiceInterfaces.AI;
using SmartCare.Application.ExternalServiceInterfaces.AI.Response;
using SmartCare.Application.Features.AI.DrugInformationExtractor;

namespace SmartCare.UnitTests.Features.AI;

public class DrugExtractorQueryHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldReturnFailed_WhenAIServiceReturnsNull()
    {
        var aiService = new Mock<IAiServices>();
        aiService.Setup(x => x.DrugInformationExtractorAsync(
                It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DrugExtractionResponse?)null);

        var sut = new DrugInformationExtractorQueryHandler(
            ResponseHandler,
            Mock.Of<ILogger<DrugInformationExtractorQueryHandler>>(),
            Mock.Of<IUnitOfWork>(), aiService.Object, Mock.Of<IMapper>());

        var result = await sut.Handle(
            new DrugInformationExtractorQuery(Mock.Of<Microsoft.AspNetCore.Http.IFormFile>()), CT);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Contain("Failed to get a response");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenAIServiceReturnsResponse()
    {
        var response = new DrugExtractionResponse(
            Detections: new List<DetectionItem>
            {
                new DetectionItem(BBox: new List<int> { 10, 20, 100, 200 }, Confidence: 0.95f)
            },
            ActiveIngredients: new List<string> { "Paracetamol", "Caffeine" });

        var aiService = new Mock<IAiServices>();
        aiService.Setup(x => x.DrugInformationExtractorAsync(
                It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var sut = new DrugInformationExtractorQueryHandler(
            ResponseHandler,
            Mock.Of<ILogger<DrugInformationExtractorQueryHandler>>(),
            Mock.Of<IUnitOfWork>(), aiService.Object, Mock.Of<IMapper>());

        var result = await sut.Handle(
            new DrugInformationExtractorQuery(Mock.Of<Microsoft.AspNetCore.Http.IFormFile>()), CT);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.ActiveIngredients.Should().Contain("Paracetamol");
        result.Data.Detections.Should().HaveCount(1);
    }
}
