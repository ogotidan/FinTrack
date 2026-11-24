using System.ComponentModel.DataAnnotations;

namespace FinTrack.Api.Models;

public class RegisterRequest
{
    [Required, StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 6,
        ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public long AnalystId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class PortfolioRequest
{
    [Required, StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(150, MinimumLength = 2)]
    public string ClientName { get; set; } = string.Empty;

    [Required, RegularExpression("^(KES|USD)$",
        ErrorMessage = "Base currency must be KES or USD.")]
    public string BaseCurrency { get; set; } = "KES";
}

public class TransactionRequest
{
    [Required, StringLength(30, MinimumLength = 1)]
    public string Instrument { get; set; } = string.Empty;

    [Required, RegularExpression("^(BUY|SELL)$",
        ErrorMessage = "Type must be BUY or SELL.")]
    public string Type { get; set; } = string.Empty;

    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    [Required]
    public DateTime TradeDate { get; set; }
}
