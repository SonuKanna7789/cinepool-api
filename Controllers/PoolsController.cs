using System.Security.Claims;
using CinePool.API.DTOs;
using CinePool.API.Models;
using CinePool.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CinePool.API.Controllers;

[ApiController]
[Route("api/pools")]
[Produces("application/json")]
public class PoolsController : ControllerBase
{
    private readonly IPoolService _poolService;

    public PoolsController(IPoolService poolService)
    {
        _poolService = poolService;
    }

    /// <summary>List OTT pools — optionally filter by platform or country</summary>
    [HttpGet]
    [SwaggerOperation(Summary = "List OTT pools", Tags = new[] { "Pools" })]
    [ProducesResponseType(typeof(PagedResult<PoolDto>), 200)]
    public async Task<IActionResult> GetPools(
        [FromQuery] Platform? platform,
        [FromQuery] string? country,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20)
    {
        size = Math.Clamp(size, 1, 50);
        page = Math.Max(1, page);
        var result = await _poolService.GetPoolsAsync(platform, country, page, size);
        return Ok(result);
    }

    /// <summary>Create a new OTT subscription pool</summary>
    [HttpPost]
    [Authorize]
    [SwaggerOperation(Summary = "Create an OTT pool", Tags = new[] { "Pools" })]
    [ProducesResponseType(typeof(PoolDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CreatePool([FromBody] CreatePoolRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _poolService.CreatePoolAsync(userId, request);
        return CreatedAtAction(nameof(GetPools), new { id = result.Id }, result);
    }

    /// <summary>Join an existing OTT pool</summary>
    [HttpPost("{id:int}/join")]
    [Authorize]
    [SwaggerOperation(Summary = "Join a pool", Tags = new[] { "Pools" })]
    [ProducesResponseType(typeof(PoolDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> JoinPool([FromRoute] int id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _poolService.JoinPoolAsync(userId, id);
        return Ok(result);
    }
}
