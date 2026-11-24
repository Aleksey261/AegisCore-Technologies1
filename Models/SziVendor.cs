namespace AegisCore.Models;

public class SziVendor
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ShortName { get; set; } = "";
    public string? FullName { get; set; }
    public string? Address { get; set; }
    public string? Inn { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}