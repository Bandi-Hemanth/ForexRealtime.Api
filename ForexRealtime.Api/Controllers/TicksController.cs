using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ForexRealtime.Api.Features.Ticks;

namespace ForexRealtime.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TicksController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetRecentTicks([FromQuery] string? symbol, [FromQuery] int limit = 100)
    {
        var result = await _mediator.Send(new GetRecentTicksQuery(symbol, limit));
        return Ok(result);
    }
}
