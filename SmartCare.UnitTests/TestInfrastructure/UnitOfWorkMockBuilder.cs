using Microsoft.AspNetCore.Identity;
using Moq;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;

namespace SmartCare.UnitTests.TestInfrastructure;

/// <summary>
/// Fluent builder for creating IUnitOfWork mocks with specific repository setups.
/// Dramatically reduces boilerplate in test arrange sections.
/// </summary>
public class UnitOfWorkMockBuilder
{
    private readonly Mock<IUnitOfWork> _mock = new();

    public UnitOfWorkMockBuilder WithCarts(ICartRepository repo)
    {
        _mock.SetupGet(x => x.Carts).Returns(repo);
        return this;
    }

    public UnitOfWorkMockBuilder WithProducts(IProductRepository repo)
    {
        _mock.SetupGet(x => x.Products).Returns(repo);
        return this;
    }

    public UnitOfWorkMockBuilder WithCategories(ICategoryRepository repo)
    {
        _mock.SetupGet(x => x.Categories).Returns(repo);
        return this;
    }

    public UnitOfWorkMockBuilder WithCompanies(ICompanyRepository repo)
    {
        _mock.SetupGet(x => x.Companies).Returns(repo);
        return this;
    }

    public UnitOfWorkMockBuilder WithClients(IClientRepository repo)
    {
        _mock.SetupGet(x => x.Clients).Returns(repo);
        return this;
    }

    public UnitOfWorkMockBuilder WithOrders(IOrderRepository repo)
    {
        _mock.SetupGet(x => x.Orders).Returns(repo);
        return this;
    }

    public UnitOfWorkMockBuilder WithInventories(IInventoryRepository repo)
    {
        _mock.SetupGet(x => x.Inventories).Returns(repo);
        return this;
    }

    public UnitOfWorkMockBuilder WithPayments(IPaymentRepository repo)
    {
        _mock.SetupGet(x => x.Payments).Returns(repo);
        return this;
    }

    public UnitOfWorkMockBuilder WithRates(IRateRepository repo)
    {
        _mock.SetupGet(x => x.Rates).Returns(repo);
        return this;
    }

    public UnitOfWorkMockBuilder WithFavourites(IFavouriteRepository repo)
    {
        _mock.SetupGet(x => x.Favourites).Returns(repo);
        return this;
    }

    public UnitOfWorkMockBuilder WithAddresses(IAddressRepository repo)
    {
        _mock.SetupGet(x => x.Addresses).Returns(repo);
        return this;
    }

    public UnitOfWorkMockBuilder WithStores(IStoreRepository repo)
    {
        _mock.SetupGet(x => x.Stores).Returns(repo);
        return this;
    }

    public UnitOfWorkMockBuilder WithPharmacists(IPharmacistRepository repo)
    {
        _mock.SetupGet(x => x.Pharmacists).Returns(repo);
        return this;
    }

    public UnitOfWorkMockBuilder WithSales(ISalesRepository repo)
    {
        _mock.SetupGet(x => x.Sales).Returns(repo);
        return this;
    }

    public UnitOfWorkMockBuilder WithEmailVerifications(IEmailVerificationRepository repo)
    {
        _mock.SetupGet(x => x.EmailVerifications).Returns(repo);
        return this;
    }

    public UnitOfWorkMockBuilder WithUserManager(UserManager<ApplictionUser> userManager)
    {
        _mock.SetupGet(x => x.UserManager).Returns(userManager);
        return this;
    }

    public UnitOfWorkMockBuilder WithSaveChanges(int result = 1)
    {
        _mock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(result);
        return this;
    }

    /// <summary>
    /// Returns the underlying Mock for additional custom setups.
    /// </summary>
    public Mock<IUnitOfWork> AsMock() => _mock;

    /// <summary>
    /// Builds the IUnitOfWork mock object.
    /// </summary>
    public IUnitOfWork Build() => _mock.Object;
}
