using System.Security.Claims;
using CinePool.API.DTOs;
using CinePool.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CinePool.API.Controllers;

[ApiController]
[Route("api/movies")]
[Produces("application/json")]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    /// <summary>Search movies by title, genre, or director</summary>
    [HttpGet("search")]
    [SwaggerOperation(Summary = "Search movies", Tags = new[] { "Movies" })]
    [ProducesResponseType(typeof(PagedResult<MovieDto>), 200)]
    public async Task<IActionResult> Search(
        [FromQuery] string? q,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20)
    {
        size = Math.Clamp(size, 1, 50);
        page = Math.Max(1, page);
        var result = await _movieService.SearchAsync(q, page, size);
        return Ok(result);
    }

    /// <summary>Get detailed movie information</summary>
    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Get movie details", Tags = new[] { "Movies" })]
    [ProducesResponseType(typeof(MovieDetailDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var result = await _movieService.GetByIdAsync(id);
        return Ok(result);
    }

    /// <summary>Rate or mark a movie as watched</summary>
    [HttpPost("{id:int}/rate")]
    [Authorize]
    [SwaggerOperation(Summary = "Rate/mark a movie as watched", Tags = new[] { "Movies" })]
    [ProducesResponseType(typeof(WatchedMovieDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RateMovie(
        [FromRoute] int id,
        [FromBody] RateMovieRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _movieService.RateOrMarkWatchedAsync(userId, id, request);
        return Ok(result);
    }
}
