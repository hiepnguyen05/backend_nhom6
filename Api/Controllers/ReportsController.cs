using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserReportService.Application.Features.Reports.Queries;

namespace UserReportService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("daily-revenue")]
    public async Task<IActionResult> GetDailyRevenue([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var result = await _mediator.Send(new GetDailyRevenueQuery(startDate, endDate));
        return Ok(result);
    }

    [HttpGet("top-selling-products")]
    public async Task<IActionResult> GetTopSellingProducts([FromQuery] int topN = 10, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var result = await _mediator.Send(new GetTopProductsQuery(topN, startDate, endDate));
        return Ok(result);
    }

    [HttpGet("top-customers")]
    public async Task<IActionResult> GetTopCustomers([FromQuery] int topN = 10)
    {
        var result = await _mediator.Send(new GetTopCustomersQuery(topN));
        return Ok(result);
    }
}
