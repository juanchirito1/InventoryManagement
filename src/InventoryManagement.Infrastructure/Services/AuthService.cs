using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using InventoryManagement.Application.DTOs.Auth;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Exceptions;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace InventoryManagement.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var normalizedUser = dto.Username.Trim().ToLower();
        var exists = await _context.Users.AnyAsync(u => u.Username.ToLower() == normalizedUser);
        if (exists)
            throw new DomainException($"El nombre de usuario '{dto.Username}' ya está registrado.");

        CreatePasswordHash(dto.Password, out byte[] passwordHash, out byte[] passwordSalt);

        var user = new User
        {
            Username = dto.Username.Trim(),
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            FullName = dto.FullName.Trim(),
            Role = string.Equals(dto.Role, "Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Seller"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var (token, expiresAt) = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            Token = token,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role,
            ExpiresAt = expiresAt
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var normalizedUser = dto.Username.Trim().ToLower();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == normalizedUser);

        if (user == null || !VerifyPasswordHash(dto.Password, user.PasswordHash, user.PasswordSalt))
            throw new DomainException("Credenciales inválidas. Usuario o contraseña incorrectos.");

        var (token, expiresAt) = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            Token = token,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role,
            ExpiresAt = expiresAt
        };
    }

    private (string token, DateTime expiresAt) GenerateJwtToken(User user)
    {
        var secret = _configuration["Jwt:SecretKey"] ?? "ClaveSecretaSuperSeguraParaInventoryManagement2026!";
        var issuer = _configuration["Jwt:Issuer"] ?? "InventoryManagement";
        var audience = _configuration["Jwt:Audience"] ?? "InventoryManagementUsers";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddHours(8);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.GivenName, user.FullName),
            new(ClaimTypes.Role, user.Role)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return (tokenHandler.WriteToken(token), expiresAt);
    }

    public static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512();
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    public static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
    {
        using var hmac = new HMACSHA512(storedSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
    }
}
