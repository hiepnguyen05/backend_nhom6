using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserReportService.Application.DTOs;
using UserReportService.Application.Features.Auth.Commands;

namespace UserReportService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _mediator.Send(new LoginCommand(request.Email, request.Password));
        return Ok(response);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var response = await _mediator.Send(new RegisterCommand(request.Email, request.Password, request.FullName, request.Phone));
        return Ok(response);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshRequest request)
    {
        var response = await _mediator.Send(new RefreshTokenCommand(request.RefreshToken));
        return Ok(response);
    }
}
