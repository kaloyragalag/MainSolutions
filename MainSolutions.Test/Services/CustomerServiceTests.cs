using FluentAssertions;
using MainSolutions.API.Models;
using MainSolutions.API.Repositories.Interfaces;
using MainSolutions.API.Services;
using Moq;
using Xunit;

namespace MainSolutions.Test.Services;

public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _repositoryMock;
    private readonly CustomerService _service;

    public CustomerServiceTests()
    {
        _repositoryMock = new Mock<ICustomerRepository>();
        _service = new CustomerService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsCustomer_WhenCustomerExists()
    {
        // Arrange
        var userId = 1;
        var expectedCustomer = new Customer
        {
            Id = 10,
            UserId = userId,
            FirstName = "Juan",
            LastName = "Dela Cruz"
        };

        _repositoryMock
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(expectedCustomer);

        // Act
        var result = await _service.GetByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.FirstName.Should().Be("Juan");
        _repositoryMock.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsNull_WhenCustomerDoesNotExist()
    {
        // Arrange
        var userId = 99;

        _repositoryMock
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync((Customer?)null);

        // Act
        var result = await _service.GetByUserIdAsync(userId);

        // Assert
        result.Should().BeNull();
        _repositoryMock.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
    }
}
