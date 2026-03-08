using CinePool.API.DTOs;
using CinePool.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CinePool.API.Controllers;

[ApiController]
[Route("api/feed")]
[Produces("application/json")]
public class FeedController : ControllerBase
{
    private readonly IFeedService _feedService;

    public FeedController(IFeedService feedService)
    {
        _feedService = feedService;
    }

    /// <summary>Get paginated social feed (reviews + boosts)</summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Get paginated social feed", Tags = new[] { "Feed" })]
    [ProducesResponseType(typeof(PagedResult<FeedItemDto>), 200)]
    public async Task<IActionResult> GetFeed(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20)
    {
        size = Math.Clamp(size, 1, 100);
        page = Math.Max(1, page);
        var result = await _feedService.GetFeedAsync(page, size);
        return Ok(result);
    }
}
