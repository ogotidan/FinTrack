namespace FinTrack.Api.Models;

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
