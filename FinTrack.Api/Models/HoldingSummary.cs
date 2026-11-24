namespace FinTrack.Api.Models;

public class HoldingSummary
{
    public string Instrument { get; set; } = string.Empty;
    public decimal NetQuantity { get; set; }
    public decimal AverageCost { get; set; }
    public decimal TotalInvested { get; set; }
    public decimal LatestPrice { get; set; }
    public decimal TotalValue { get; set; }
}
