using System.Security.Claims;
using CinePool.API.DTOs;
using CinePool.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CinePool.API.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
[Produces("application/json")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    /// <summary>Get current user profile with stats</summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Get current user profile", Tags = new[] { "Profile" })]
    [ProducesResponseType(typeof(ProfileDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();
        var result = await _profileService.GetProfileAsync(userId);
        return Ok(result);
    }

    /// <summary>Get current user's watched movies (paginated)</summary>
    [HttpGet("watched")]
    [SwaggerOperation(Summary = "Get watched movies", Tags = new[] { "Profile" })]
    [ProducesResponseType(typeof(PagedResult<WatchedMovieDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetWatched(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20)
    {
        size = Math.Clamp(size, 1, 50);
        page = Math.Max(1, page);
        var userId = GetUserId();
        var result = await _profileService.GetWatchedAsync(userId, page, size);
        return Ok(result);
    }

    /// <summary>Get user preferences (genres + platforms)</summary>
    [HttpGet("preferences")]
    [SwaggerOperation(Summary = "Get user preferences", Tags = new[] { "Profile" })]
    [ProducesResponseType(typeof(UserPreferencesDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetPreferences()
    {
        var userId = GetUserId();
        var result = await _profileService.GetPreferencesAsync(userId);
        return Ok(result);
    }

    /// <summary>Update user preferences (favorite genres and platforms)</summary>
    [HttpPut("preferences")]
    [SwaggerOperation(Summary = "Update user preferences", Tags = new[] { "Profile" })]
    [ProducesResponseType(typeof(UserPreferencesDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferencesRequest request)
    {
        var userId = GetUserId();
        var result = await _profileService.UpdatePreferencesAsync(userId, request);
        return Ok(result);
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

[ApiController]
[Route("api/suggestions")]
[Authorize]
[Produces("application/json")]
public class SuggestionsController : ControllerBase
{
    private readonly IProfileService _profileService;

    public SuggestionsController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    /// <summary>
    /// Get AI-ready user context — watch history, preferences, and rating averages.
    /// This endpoint is consumed by the AI suggestion micro-service.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get AI suggestion context",
        Description = "Returns a rich context object including watch history, preferences, and genre affinity. Consumed by the AI suggestion service.",
        Tags = new[] { "Suggestions" }
    )]
    [ProducesResponseType(typeof(SuggestionContextDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetSuggestions()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _profileService.GetSuggestionContextAsync(userId);
        return Ok(result);
    }
}
