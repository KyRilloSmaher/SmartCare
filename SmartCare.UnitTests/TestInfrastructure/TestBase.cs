using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.Handlers.ResponsesHandler;

namespace SmartCare.UnitTests.TestInfrastructure;

/// <summary>
/// Base class for all handler unit tests.
/// Provides shared infrastructure: ResponseHandler, CancellationToken, etc.
/// </summary>
public abstract class TestBase
{
    /// <summary>
    /// A real ResponseHandler instance (stateless, safe to share).
    /// </summary>
    protected static readonly IResponseHandler ResponseHandler = new ResponseHandler();

    /// <summary>
    /// Convenience shorthand for CancellationToken.None.
    /// </summary>
    protected static readonly CancellationToken CT = CancellationToken.None;
}
