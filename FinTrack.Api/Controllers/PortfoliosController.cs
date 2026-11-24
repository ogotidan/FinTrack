using FinTrack.Api.Helpers;
using FinTrack.Api.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace FinTrack.Api.Controllers;

[Route("api/portfolios")]
public class PortfoliosController : BaseApiController
{
    private readonly DBHandler _db;
    public PortfoliosController(DBHandler db) => _db = db;

    [HttpGet]
    public IActionResult GetAll()
        => Ok(ApiResponse<List<Portfolio>>.Ok(_db.GetPortfolios(AnalystId)));

    [HttpGet("{id:long}")]
    public IActionResult GetOne(long id)
    {
        var p = _db.GetPortfolioById(id, AnalystId);
        return p is null
            ? NotFound(new ApiError("Portfolio not found."))
            : Ok(ApiResponse<Portfolio>.Ok(p));
    }

    [HttpPost]
    public IActionResult Create([FromBody] PortfolioRequest req)
    {
        var id = _db.AddPortfolio(AnalystId, req.Name.Trim(), req.ClientName.Trim(), req.BaseCurrency);
        var created = _db.GetPortfolioById(id, AnalystId);
        return Created($"/api/portfolios/{id}",
            ApiResponse<Portfolio>.Ok(created!, "Portfolio created."));
    }

    [HttpPut("{id:long}")]
    public IActionResult Update(long id, [FromBody] PortfolioRequest req)
    {
        var rows = _db.UpdatePortfolio(id, AnalystId, req.Name.Trim(), req.ClientName.Trim(), req.BaseCurrency);
        if (rows == 0) return NotFound(new ApiError("Portfolio not found."));
        return Ok(ApiResponse<Portfolio>.Ok(_db.GetPortfolioById(id, AnalystId)!, "Portfolio updated."));
    }

    [HttpDelete("{id:long}")]
    public IActionResult Delete(long id)
    {
        var rows = _db.DeletePortfolio(id, AnalystId);
        return rows == 0
            ? NotFound(new ApiError("Portfolio not found."))
            : Ok(ApiResponse<object>.Ok(new { id }, "Portfolio deleted."));
    }

    [HttpGet("{id:long}/transactions")]
    public IActionResult GetTransactions(long id)
    {
        if (_db.GetPortfolioById(id, AnalystId) is null)
            return NotFound(new ApiError("Portfolio not found."));
        return Ok(ApiResponse<List<Transaction>>.Ok(_db.GetTransactions(id)));
    }

    [HttpPost("{id:long}/transactions")]
    public IActionResult AddTransaction(long id, [FromBody] TransactionRequest req)
    {
        if (_db.GetPortfolioById(id, AnalystId) is null)
            return NotFound(new ApiError("Portfolio not found."));

        if (req.Quantity <= 0)
            return BadRequest(new ApiError("Quantity must be greater than zero."));
        if (req.UnitPrice < 0)
            return BadRequest(new ApiError("Unit price cannot be negative."));

        try
        {
            var txId = _db.AddTransaction(
                id,
                req.Instrument.Trim().ToUpperInvariant(),
                req.Type.ToUpperInvariant(),
                req.Quantity,
                req.UnitPrice,
                req.TradeDate);

            return Created($"/api/portfolios/{id}/transactions",
                ApiResponse<object>.Ok(new { id = txId }, "Transaction recorded."));
        }
        catch (MySqlException ex) when (ex.Number == 1644)
        {
            return BadRequest(new ApiError(ex.Message));
        }
    }

    [HttpGet("{id:long}/summary")]
    public IActionResult GetSummary(long id)
    {
        if (_db.GetPortfolioById(id, AnalystId) is null)
            return NotFound(new ApiError("Portfolio not found."));
        return Ok(ApiResponse<List<HoldingSummary>>.Ok(_db.GetPortfolioSummary(id)));
    }
}
