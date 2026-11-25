using System.ComponentModel.DataAnnotations;

namespace FinTrack.Web.Models;

public class ApiResult<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }

    public static ApiResult<T> Ok(T? data, string? message = null)
        => new() { Success = true, Data = data, Message = message };

    public static ApiResult<T> Fail(string message)
        => new() { Success = false, Message = message };
}

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    [Required, StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = string.Empty;
}

public class AuthResult
{
    public string Token { get; set; } = string.Empty;
    public long AnalystId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class Portfolio
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string BaseCurrency { get; set; } = "KES";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int TransactionCount { get; set; }
}

public class PortfolioRequest
{
    [Required, StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(150, MinimumLength = 2)]
    public string ClientName { get; set; } = string.Empty;

    [Required]
    public string BaseCurrency { get; set; } = "KES";
}

public class Transaction
{
    public long Id { get; set; }
    public long PortfolioId { get; set; }
    public string Instrument { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime TradeDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TransactionRequest
{
    [Required]
    public string Instrument { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = "BUY";

    [Range(0.0001, 1000000000, ErrorMessage = "Quantity must be greater than zero.")]
    public decimal Quantity { get; set; }

    [Range(0, 1000000000, ErrorMessage = "Unit price cannot be negative.")]
    public decimal UnitPrice { get; set; }

    [Required]
    public DateTime TradeDate { get; set; } = DateTime.Today;
}

public class HoldingSummary
{
    public string Instrument { get; set; } = string.Empty;
    public decimal NetQuantity { get; set; }
    public decimal AverageCost { get; set; }
    public decimal TotalInvested { get; set; }
    public decimal LatestPrice { get; set; }
    public decimal TotalValue { get; set; }
}
