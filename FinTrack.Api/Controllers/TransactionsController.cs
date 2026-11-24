using FinTrack.Api.Helpers;
using FinTrack.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Api.Controllers;

[Route("api/transactions")]
public class TransactionsController : BaseApiController
{
    private readonly DBHandler _db;
    public TransactionsController(DBHandler db) => _db = db;

    [HttpDelete("{id:long}")]
    public IActionResult Delete(long id)
    {
        var rows = _db.DeleteTransaction(id, AnalystId);
        return rows == 0
            ? NotFound(new ApiError("Transaction not found."))
            : Ok(ApiResponse<object>.Ok(new { id }, "Transaction deleted."));
    }
}
