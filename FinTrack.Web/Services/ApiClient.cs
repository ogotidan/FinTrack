using System.Net.Http.Headers;
using System.Net.Http.Json;
using FinTrack.Web.Models;

namespace FinTrack.Web.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly TokenStore _tokens;

    public ApiClient(HttpClient http, TokenStore tokens)
    {
        _http = http;
        _tokens = tokens;
    }

    private async Task AttachTokenAsync()
    {
        var token = await _tokens.GetTokenAsync();
        _http.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }

    private sealed class Envelope<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }

    private async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string url, object? body, bool auth)
    {
        try
        {
            if (auth) await AttachTokenAsync();

            using var request = new HttpRequestMessage(method, url);
            if (body is not null) request.Content = JsonContent.Create(body);

            var response = await _http.SendAsync(request);

            Envelope<T>? env = null;
            try { env = await response.Content.ReadFromJsonAsync<Envelope<T>>(); }
            catch { /* non-JSON body */ }

            if (response.IsSuccessStatusCode)
                return ApiResult<T>.Ok(env is not null ? env.Data : default, env?.Message);

            return ApiResult<T>.Fail(env?.Message ?? $"Request failed ({(int)response.StatusCode}).");
        }
        catch (Exception)
        {
            return ApiResult<T>.Fail("Could not reach the server. Is the API running?");
        }
    }

    // ---- Auth ----
    public Task<ApiResult<AuthResult>> LoginAsync(LoginRequest req)
        => SendAsync<AuthResult>(HttpMethod.Post, "api/auth/login", req, auth: false);

    public Task<ApiResult<AuthResult>> RegisterAsync(RegisterRequest req)
        => SendAsync<AuthResult>(HttpMethod.Post, "api/auth/register", req, auth: false);

    // ---- Portfolios ----
    public Task<ApiResult<List<Portfolio>>> GetPortfoliosAsync()
        => SendAsync<List<Portfolio>>(HttpMethod.Get, "api/portfolios", null, auth: true);

    public Task<ApiResult<Portfolio>> GetPortfolioAsync(long id)
        => SendAsync<Portfolio>(HttpMethod.Get, $"api/portfolios/{id}", null, auth: true);

    public Task<ApiResult<Portfolio>> CreatePortfolioAsync(PortfolioRequest req)
        => SendAsync<Portfolio>(HttpMethod.Post, "api/portfolios", req, auth: true);

    public Task<ApiResult<Portfolio>> UpdatePortfolioAsync(long id, PortfolioRequest req)
        => SendAsync<Portfolio>(HttpMethod.Put, $"api/portfolios/{id}", req, auth: true);

    public Task<ApiResult<object>> DeletePortfolioAsync(long id)
        => SendAsync<object>(HttpMethod.Delete, $"api/portfolios/{id}", null, auth: true);

    // ---- Transactions ----
    public Task<ApiResult<List<Transaction>>> GetTransactionsAsync(long portfolioId)
        => SendAsync<List<Transaction>>(HttpMethod.Get, $"api/portfolios/{portfolioId}/transactions", null, auth: true);

    public Task<ApiResult<object>> AddTransactionAsync(long portfolioId, TransactionRequest req)
        => SendAsync<object>(HttpMethod.Post, $"api/portfolios/{portfolioId}/transactions", req, auth: true);

    public Task<ApiResult<object>> DeleteTransactionAsync(long id)
        => SendAsync<object>(HttpMethod.Delete, $"api/transactions/{id}", null, auth: true);

    // ---- Summary ----
    public Task<ApiResult<List<HoldingSummary>>> GetSummaryAsync(long portfolioId)
        => SendAsync<List<HoldingSummary>>(HttpMethod.Get, $"api/portfolios/{portfolioId}/summary", null, auth: true);
}
