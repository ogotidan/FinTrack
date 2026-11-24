namespace FinTrack.Api.Models;

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
