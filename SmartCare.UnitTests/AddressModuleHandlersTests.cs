using AutoMapper;
using Moq;
using SmartCare.Application.CQRs.Address.Commands;
using SmartCare.Application.CQRs.Address.Extensions;
using SmartCare.Application.CQRs.Address.Handlers;
using SmartCare.Application.CQRs.Address.Queries;
using SmartCare.Application.DTOs.Address.Requests;
using SmartCare.Application.DTOs.Address.Responses;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;

namespace SmartCare.UnitTests.Features;

public class AddressModuleHandlersTests
{
    [Fact]
    public async Task AddAddress_ShouldReturnNotFound_WhenClientMissing()
    {
        var clients = new Mock<IClientRepository>();
        clients.Setup(x => x.GetByIdAsync("c1", false)).ReturnsAsync((Client?)null);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Clients).Returns(clients.Object);

        var sut = new AddNewClientAddressHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<IRedisCacheService>(), Mock.Of<IMapper>(), uow.Object);
        var result = await sut.Handle(new AddNewClientAddressAsyncCommand("c1", new CreateAddressRequestDto()), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.USER_NOT_FOUND);
    }

    [Fact]
    public async Task DeleteAddress_ShouldReturnNotFound_WhenAddressNotOwned()
    {
        var client = new Client { Id = "c1" };
        var clients = new Mock<IClientRepository>();
        clients.Setup(x => x.GetByIdAsync("c1", false)).ReturnsAsync(client);

        var addresses = new Mock<IAddressRepository>();
        addresses.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), true)).ReturnsAsync(new Address { Id = Guid.NewGuid(), ClientId = "other" });

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Clients).Returns(clients.Object);
        uow.SetupGet(x => x.Addresses).Returns(addresses.Object);

        var sut = new DeleteClientAddressHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), Mock.Of<IRedisCacheService>(), Mock.Of<IMapper>(), uow.Object);
        var result = await sut.Handle(new DeleteClientAddressAsyncCommand("c1", Guid.NewGuid()), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.ADDRESS_NOT_FOUND);
    }

    [Fact]
    public async Task GetClientAddresses_ShouldReturnCached_WhenCacheHit()
    {
        var client = new Client { Id = "c1" };
        var clients = new Mock<IClientRepository>();
        clients.Setup(x => x.GetByIdAsync("c1", false)).ReturnsAsync(client);

        var cache = new Mock<IRedisCacheService>();
        cache.Setup(x => x.GetDataAsync<IEnumerable<AddressResponseDto>>("client_addresses_c1", CacheConstants.Addresses))
            .ReturnsAsync(new List<AddressResponseDto> { new AddressResponseDto { Id = Guid.NewGuid() } });

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Clients).Returns(clients.Object);

        var sut = new GetClientAddressesHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), cache.Object, Mock.Of<IMapper>(), uow.Object);
        var result = await sut.Handle(new GetClientAddressesAsyncQuery("c1"), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task SetPrimaryAddress_ShouldReturnNotFound_WhenAddressMissing()
    {
        var client = new Client { Id = "c1" };
        var clients = new Mock<IClientRepository>();
        clients.Setup(x => x.GetByIdAsync("c1", false)).ReturnsAsync(client);

        var addresses = new Mock<IAddressRepository>();
        addresses.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), true)).ReturnsAsync((Address?)null);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Clients).Returns(clients.Object);
        uow.SetupGet(x => x.Addresses).Returns(addresses.Object);

        var sut = new SetAddressAsPrimaryAddressHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), clients.Object, addresses.Object, Mock.Of<IRedisCacheService>(), Mock.Of<IMapper>(), uow.Object);
        var result = await sut.Handle(new SetAddressAsPrimaryAddressAsyncCommand("c1", Guid.NewGuid()), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.ADDRESS_NOT_FOUND);
    }

    [Fact]
    public async Task UpdateAddress_ShouldReturnNotFound_WhenAddressMissing()
    {
        // Arrange
        var client = new Client { Id = "c1" };
        var clients = new Mock<IClientRepository>();
        clients.Setup(x => x.GetValidClientAsync("c1")).ReturnsAsync(client); 

        var addresses = new Mock<IAddressRepository>();
        addresses.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), true)).ReturnsAsync((Address?)null);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Clients).Returns(clients.Object);
        uow.SetupGet(x => x.Addresses).Returns(addresses.Object);

        var sut = new UpdateClientAddressHandler(new SmartCare.Application.Handlers.ResponsesHandler.ResponseHandler(), clients.Object, addresses.Object, Mock.Of<IRedisCacheService>(), Mock.Of<IMapper>(), uow.Object);
        var result = await sut.Handle(new UpdateClientAddressAsyncCommand("c1", new UpdateAddressRequestDto { Id = Guid.NewGuid(), IsPrimary = true }), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be(SystemMessages.ADDRESS_NOT_FOUND);
    }
}
