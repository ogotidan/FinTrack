using FinTrack.Web.Models;

namespace FinTrack.Web.Services;

public class PortfolioStore
{
    private readonly ApiClient _api;

    public PortfolioStore(ApiClient api) => _api = api;

    public List<Portfolio> Items { get; private set; } = new();
    public bool Loaded { get; private set; }
    public event Action? Changed;

    public async Task RefreshAsync()
    {
        var res = await _api.GetPortfoliosAsync();
        if (res.Success && res.Data is not null)
        {
            Items = res.Data;
            Loaded = true;
            Changed?.Invoke();
        }
    }

    public void Clear()
    {
        Items = new();
        Loaded = false;
        Changed?.Invoke();
    }
}
