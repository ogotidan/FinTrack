using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Api.Controllers;

[ApiController]
[Authorize]
public abstract class BaseApiController : ControllerBase
{
    protected long AnalystId =>
        long.Parse(User.FindFirstValue("analyst_id")
                   ?? throw new InvalidOperationException("Missing analyst_id claim."));
}
