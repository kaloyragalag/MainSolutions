using FluentAssertions;
using MainSolutions.API.Controllers;
using MainSolutions.API.Models;
using MainSolutions.API.Repositories.Interfaces;
using MainSolutions.API.Services;
using MainSolutions.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace MainSolutions.Test.Controllers;

public class CustomerControllerTests
{
    private readonly Mock<ICustomerService> _serviceMock;
    private readonly Mock<ICustomerRepository> _repositoryMock;
    private readonly CustomerController _controller;

    public CustomerControllerTests()
    {
        _serviceMock = new Mock<ICustomerService>();
        _repositoryMock = new Mock<ICustomerRepository>();
        _controller = new CustomerController(_serviceMock.Object, _repositoryMock.Object, new ReflectionEntityPatcher());
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenUserDoesNotExist()
    {
        // Arrange
        var customer = new Customer { UserId = 1, FirstName = "Juan", LastName = "Dela Cruz" };

        _repositoryMock
            .Setup(r => r.UserExistsAsync(customer.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Create(customer, CancellationToken.None);

        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().BeEquivalentTo(new
        {
            message = $"User with id {customer.UserId} does not exist."
        });

        _repositoryMock.Verify(r => r.UserExistsAsync(customer.UserId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_ReturnsConflict_WhenUserAlreadyHasCustomer()
    {
        // Arrange
        var customer = new Customer { UserId = 1, FirstName = "Juan", LastName = "Dela Cruz" };
        var existingCustomer = new Customer { Id = 5, UserId = 1, FirstName = "Existing", LastName = "Customer" };

        _repositoryMock
            .Setup(r => r.UserExistsAsync(customer.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(r => r.GetByUserIdAsync(customer.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCustomer);

        // Act
        var result = await _controller.Create(customer, CancellationToken.None);

        // Assert
        var conflict = result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflict.Value.Should().BeEquivalentTo(new
        {
            message = $"User with id {customer.UserId} is already associated with a customer."
        });

        _repositoryMock.Verify(r => r.UserExistsAsync(customer.UserId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.GetByUserIdAsync(customer.UserId, It.IsAny<CancellationToken>()), Times.Once);
        _serviceMock.Verify(s => s.CreateAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_SetsCreatedAtAndCallsBaseCreate_WhenValid()
    {
        // Arrange
        var customer = new Customer { UserId = 1, FirstName = "Juan", LastName = "Dela Cruz" };

        _repositoryMock
            .Setup(r => r.UserExistsAsync(customer.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(r => r.GetByUserIdAsync(customer.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        _serviceMock
            .Setup(s => s.CreateAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var before = DateTime.UtcNow;

        // Act
        var result = await _controller.Create(customer, CancellationToken.None);

        // Assert
        customer.CreatedAt.Should().BeOnOrAfter(before);
        customer.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);

        _serviceMock.Verify(s => s.CreateAsync(customer, It.IsAny<CancellationToken>()), Times.Once);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        var id = 1;
        var fields = new Dictionary<string, object?> { { "firstName", "Updated" } };

        _serviceMock
            .Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        // Act
        var result = await _controller.Update(id, fields, CancellationToken.None);

        // Assert
        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFound.Value.Should().BeEquivalentTo(new
        {
            message = $"Customer with id {id} was not found."
        });

        _serviceMock.Verify(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.UserExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenUserIdFieldDoesNotExist()
    {
        // Arrange
        var id = 1;
        var existing = new Customer { Id = id, UserId = 5, FirstName = "Juan", LastName = "Dela Cruz" };
        var fields = new Dictionary<string, object?> { { "userId", "999" } };

        _serviceMock
            .Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _repositoryMock
            .Setup(r => r.UserExistsAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Update(id, fields, CancellationToken.None);

        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().BeEquivalentTo(new
        {
            message = "User with id 999 does not exist."
        });

        _repositoryMock.Verify(r => r.UserExistsAsync(999, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_ReturnsOkWithUpdatedCustomer_WhenValid()
    {
        // Arrange
        var id = 1;
        var existing = new Customer { Id = id, UserId = 5, FirstName = "Juan", LastName = "Dela Cruz" };
        var updated = new Customer { Id = id, UserId = 5, FirstName = "Updated", LastName = "Dela Cruz" };
        var fields = new Dictionary<string, object?> { { "firstName", "Updated" } };

        _serviceMock
            .Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        // Act
        var result = await _controller.Update(id, fields, CancellationToken.None);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(updated);

        _repositoryMock.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
