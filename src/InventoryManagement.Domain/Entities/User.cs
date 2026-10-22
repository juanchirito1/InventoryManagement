using InventoryManagement.Domain.Common;

namespace InventoryManagement.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "Seller"; // "Admin" o "Seller"
}
