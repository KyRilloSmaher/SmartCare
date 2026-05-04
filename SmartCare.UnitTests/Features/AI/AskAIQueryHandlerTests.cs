using MediatR;
using SmartCare.Application.ExternalServiceInterfaces.AI;
using SmartCare.Application.ExternalServiceInterfaces.AI.Response;
using SmartCare.Application.Features.AI.Chat;

namespace SmartCare.UnitTests.Features.AI;

public class AskAIQueryHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldReturnFailed_WhenAIServiceReturnsNull()
    {
        var aiService = new Mock<IAiServices>();
        aiService.Setup(x => x.AskAIAsync(
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Microsoft.AspNetCore.Http.IFormFile?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AiAnswerResult?)null);

        var sut = new AskAIQueryHandler(
            ResponseHandler, Mock.Of<IMediator>(),
            Mock.Of<ILogger<AskAIQueryHandler>>(),
            Mock.Of<IUnitOfWork>(), aiService.Object, Mock.Of<IMapper>());

        var result = await sut.Handle(new AskAIQuery(null, "What is aspirin?", null), CT);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Contain("Failed to get a response");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenAIServiceReturnsResponse()
    {
        var aiResponse = new AiAnswerResult(
            ingredient: "Acetylsalicylic acid",
            question: "What is aspirin?",
            answer: "Aspirin is a pain reliever.");

        var aiService = new Mock<IAiServices>();
        aiService.Setup(x => x.AskAIAsync(
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Microsoft.AspNetCore.Http.IFormFile?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(aiResponse);

        var sut = new AskAIQueryHandler(
            ResponseHandler, Mock.Of<IMediator>(),
            Mock.Of<ILogger<AskAIQueryHandler>>(),
            Mock.Of<IUnitOfWork>(), aiService.Object, Mock.Of<IMapper>());

        var result = await sut.Handle(new AskAIQuery(null, "What is aspirin?", null), CT);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.answer.Should().Contain("pain reliever");
    }
}
