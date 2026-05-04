using SmartCare.Application.Features.DashBoard.Commands.ChangePharmacistBranch;

namespace SmartCare.UnitTests.Features.DashBoard;

public class ChangePharmacistBranchTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldReturnFailed_WhenPharmacistNotFound()
    {
        var pharmacists = new Mock<IPharmacistRepository>();
        pharmacists.Setup(x => x.GetByUserIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Pharmacist?)null);

        var uow = new UnitOfWorkMockBuilder().WithPharmacists(pharmacists.Object).Build();

        var sut = new ChangePharmacistBranchCommandHandler(
            uow, ResponseHandler, Mock.Of<ILogger<ChangePharmacistBranchCommandHandler>>());

        var result = await sut.Handle(
            new ChangePharmacistBranchCommand("p1", Guid.NewGuid()), CT);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Contain("Pharmacist not found");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailed_WhenTargetBranchNotFound()
    {
        var pharmacist = new Pharmacist { Id = Guid.NewGuid().ToString(), StoreId = Guid.NewGuid() };

        var pharmacists = new Mock<IPharmacistRepository>();
        pharmacists.Setup(x => x.GetByUserIdAsync("p1")).ReturnsAsync(pharmacist);

        var stores = new Mock<IStoreRepository>();
        stores.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), false)).ReturnsAsync((Store?)null);

        var uow = new UnitOfWorkMockBuilder()
            .WithPharmacists(pharmacists.Object)
            .WithStores(stores.Object)
            .Build();

        var sut = new ChangePharmacistBranchCommandHandler(
            uow, ResponseHandler, Mock.Of<ILogger<ChangePharmacistBranchCommandHandler>>());

        var result = await sut.Handle(
            new ChangePharmacistBranchCommand("p1", Guid.NewGuid()), CT);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Contain("Target branch not found");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailed_WhenAlreadyAssignedToSameBranch()
    {
        var branchId = Guid.NewGuid();
        var pharmacist = new Pharmacist { Id = Guid.NewGuid().ToString(), StoreId = branchId };

        var pharmacists = new Mock<IPharmacistRepository>();
        pharmacists.Setup(x => x.GetByUserIdAsync("p1")).ReturnsAsync(pharmacist);

        var stores = new Mock<IStoreRepository>();
        stores.Setup(x => x.GetByIdAsync(branchId, false)).ReturnsAsync(new Store("Branch", "Address", 0, 0, "123") { Id = branchId });

        var uow = new UnitOfWorkMockBuilder()
            .WithPharmacists(pharmacists.Object)
            .WithStores(stores.Object)
            .Build();

        var sut = new ChangePharmacistBranchCommandHandler(
            uow, ResponseHandler, Mock.Of<ILogger<ChangePharmacistBranchCommandHandler>>());

        var result = await sut.Handle(
            new ChangePharmacistBranchCommand("p1", branchId), CT);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Contain("already assigned");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenBranchChanged()
    {
        var oldBranch = Guid.NewGuid();
        var newBranch = Guid.NewGuid();
        var pharmacist = new Pharmacist { Id = Guid.NewGuid().ToString(), StoreId = oldBranch };

        var pharmacists = new Mock<IPharmacistRepository>();
        pharmacists.Setup(x => x.GetByUserIdAsync("p1")).ReturnsAsync(pharmacist);

        var stores = new Mock<IStoreRepository>();
        stores.Setup(x => x.GetByIdAsync(newBranch, false)).ReturnsAsync(new Store("Branch", "Address", 0, 0, "123") { Id = newBranch });

        var uow = new UnitOfWorkMockBuilder()
            .WithPharmacists(pharmacists.Object)
            .WithStores(stores.Object)
            .WithSaveChanges()
            .Build();

        var sut = new ChangePharmacistBranchCommandHandler(
            uow, ResponseHandler, Mock.Of<ILogger<ChangePharmacistBranchCommandHandler>>());

        var result = await sut.Handle(
            new ChangePharmacistBranchCommand("p1", newBranch), CT);

        result.Succeeded.Should().BeTrue();
        pharmacist.StoreId.Should().Be(newBranch);
    }
}
