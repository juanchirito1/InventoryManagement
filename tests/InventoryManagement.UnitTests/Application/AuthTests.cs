using FluentAssertions;
using InventoryManagement.Infrastructure.Services;
using Xunit;

namespace InventoryManagement.UnitTests.Application;

public class AuthTests
{
    [Fact]
    public void CreatePasswordHash_ShouldGenerateSaltAndHash()
    {
        // Arrange
        var password = "SuperSecretPassword123!";

        // Act
        AuthService.CreatePasswordHash(password, out byte[] hash, out byte[] salt);

        // Assert
        hash.Should().NotBeEmpty();
        salt.Should().NotBeEmpty();
    }

    [Fact]
    public void VerifyPasswordHash_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "CorrectPassword2026!";
        AuthService.CreatePasswordHash(password, out byte[] hash, out byte[] salt);

        // Act
        var isValid = AuthService.VerifyPasswordHash(password, hash, salt);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifyPasswordHash_WithWrongPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "CorrectPassword2026!";
        AuthService.CreatePasswordHash(password, out byte[] hash, out byte[] salt);

        // Act
        var isValid = AuthService.VerifyPasswordHash("WrongPassword!", hash, salt);

        // Assert
        isValid.Should().BeFalse();
    }
}
