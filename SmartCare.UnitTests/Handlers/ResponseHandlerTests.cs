using System.Net;
using SmartCare.Application.Handlers.ResponsesHandler;

namespace SmartCare.UnitTests.Handlers;

public class ResponseHandlerTests
{
    private readonly ResponseHandler _sut = new();

    [Fact]
    public void Success_ShouldReturnOkAndData()
    {
        var result = _sut.Success("payload", "done");

        result.Succeeded.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data.Should().Be("payload");
        result.Message.Should().Be("done");
    }

    [Fact]
    public void NotFound_ShouldReturnNotFoundAndFailed()
    {
        var result = _sut.NotFound<string>("missing");

        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.Message.Should().Be("missing");
    }

    [Fact]
    public void Failed_ShouldReturnFailedDependency()
    {
        var result = _sut.Failed<string>();

        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.FailedDependency);
        result.Message.Should().Be("InternalServerError");
    }
}
