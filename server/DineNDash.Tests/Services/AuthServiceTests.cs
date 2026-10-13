using DineNDash.Core.Enums;
using DineNDash.Core.Exceptions;
using DineNDash.Core.Models;
using DineNDash.Core.Services;
using DineNDash.Tests.Fakes;

namespace DineNDash.Tests.Services;

public class AuthServiceTests
{
    private AuthService _authService = null!;
    private InMemoryRepository<User> _users = null!;

    [SetUp]
    public void SetUp()
    {
        _users = new InMemoryRepository<User>();
        _authService = new AuthService(_users, new BCryptPasswordHasher());
    }

    [Test]
    public async Task Register_StoresHashedPasswordNotPlainText()
    {
        var user = await _authService.RegisterAsync("diner@example.com", "Password123", "Dana", UserRole.Diner);

        Assert.Multiple(() =>
        {
            Assert.That(user.Id, Is.GreaterThan(0));
            Assert.That(user.PasswordHash, Is.Not.EqualTo("Password123"));
            Assert.That(user.PasswordHash, Does.StartWith("$2"));
        });
    }

    [Test]
    public async Task Register_CreatesAccountTypeMatchingRole()
    {
        var diner = await _authService.RegisterAsync("diner@example.com", "Password123", "Dana", UserRole.Diner);
        var owner = await _authService.RegisterAsync("owner@example.com", "Password123", "Omar", UserRole.RestaurantOwner);

        Assert.Multiple(() =>
        {
            Assert.That(diner, Is.TypeOf<DinerAccount>());
            Assert.That(owner, Is.TypeOf<RestaurantAccount>());
        });
    }

    [Test]
    public async Task Register_DuplicateEmail_ThrowsConflict()
    {
        await _authService.RegisterAsync("diner@example.com", "Password123", "Dana", UserRole.Diner);

        Assert.ThrowsAsync<ConflictException>(() =>
            _authService.RegisterAsync("Diner@Example.com", "Another123", "Dan", UserRole.Diner));
    }

    [Test]
    public void Register_ShortPassword_ThrowsBadRequest()
    {
        Assert.ThrowsAsync<BadRequestException>(() =>
            _authService.RegisterAsync("diner@example.com", "short", "Dana", UserRole.Diner));
    }

    [Test]
    public void Register_InvalidEmail_ThrowsBadRequest()
    {
        Assert.ThrowsAsync<BadRequestException>(() =>
            _authService.RegisterAsync("not-an-email", "Password123", "Dana", UserRole.Diner));
    }

    [Test]
    public async Task Login_CorrectPassword_ReturnsUser()
    {
        await _authService.RegisterAsync("diner@example.com", "Password123", "Dana", UserRole.Diner);

        var user = await _authService.LoginAsync("diner@example.com", "Password123");

        Assert.That(user.Email, Is.EqualTo("diner@example.com"));
    }

    [Test]
    public async Task Login_WrongPassword_ThrowsUnauthorized()
    {
        await _authService.RegisterAsync("diner@example.com", "Password123", "Dana", UserRole.Diner);

        Assert.ThrowsAsync<UnauthorizedException>(() =>
            _authService.LoginAsync("diner@example.com", "WrongPassword"));
    }

    [Test]
    public void Login_UnknownEmail_ThrowsUnauthorized()
    {
        Assert.ThrowsAsync<UnauthorizedException>(() =>
            _authService.LoginAsync("nobody@example.com", "Password123"));
    }
}
