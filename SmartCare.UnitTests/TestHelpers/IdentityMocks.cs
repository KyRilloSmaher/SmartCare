using Microsoft.AspNetCore.Identity;
using Moq;
using SmartCare.Domain.Entities;

namespace SmartCare.UnitTests.TestHelpers;

internal static class IdentityMocks
{
    public static Mock<UserManager<ApplictionUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplictionUser>>();
        return new Mock<UserManager<ApplictionUser>>(
            store.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);
    }
}
