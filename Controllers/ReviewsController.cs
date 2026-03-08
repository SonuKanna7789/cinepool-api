using System.Security.Claims;
using CinePool.API.DTOs;
using CinePool.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CinePool.API.Controllers;

[ApiController]
[Route("api/reviews")]
[Authorize]
[Produces("application/json")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>Create a new movie review</summary>
    [HttpPost]
    [SwaggerOperation(Summary = "Create a movie review", Tags = new[] { "Reviews" })]
    [ProducesResponseType(typeof(ReviewDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
    {
        var userId = GetUserId();
        var result = await _reviewService.CreateAsync(userId, request);
        return CreatedAtAction(nameof(CreateReview), new { id = result.Id }, result);
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

[ApiController]
[Route("api/boosts")]
[Authorize]
[Produces("application/json")]
public class BoostsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public BoostsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>Boost a review</summary>
    [HttpPost]
    [SwaggerOperation(Summary = "Boost a review", Tags = new[] { "Boosts" })]
    [ProducesResponseType(typeof(BoostDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> BoostReview([FromBody] CreateBoostRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _reviewService.BoostAsync(userId, request);
        return CreatedAtAction(nameof(BoostReview), new { id = result.Id }, result);
    }
}
