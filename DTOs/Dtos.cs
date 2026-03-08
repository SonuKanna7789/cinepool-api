using CinePool.API.Models;

namespace CinePool.API.DTOs;

// ─── Auth (these are manually constructed — positional records are fine) ──────

public record RegisterRequest(string Name, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string AccessToken, string RefreshToken, UserDto User);
public record RefreshTokenRequest(string RefreshToken);

// ─── User ─────────────────────────────────────────────────────────────────────

public class UserDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Avatar { get; init; }
    public bool IsEnthusiast { get; init; }
    public DateTime CreatedAt { get; init; }
}

public class ProfileDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Avatar { get; init; }
    public bool IsEnthusiast { get; init; }
    public DateTime CreatedAt { get; init; }
    public int ReviewCount { get; init; }
    public int WatchedCount { get; init; }
    public int BoostCount { get; init; }
    public int PoolCount { get; init; }
}

// ─── Movie ────────────────────────────────────────────────────────────────────

public class MovieDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public int Year { get; init; }
    public string? PosterUrl { get; init; }
    public string? Genre { get; init; }
    public string? Director { get; init; }
    public double Rating { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<string> Platforms { get; init; } = new();
}

public class MovieDetailDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public int Year { get; init; }
    public string? PosterUrl { get; init; }
    public string? Genre { get; init; }
    public string? Director { get; init; }
    public double Rating { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<string> Platforms { get; init; } = new();
    public List<ReviewDto> RecentReviews { get; init; } = new();
    public int TotalReviews { get; init; }
    public double AverageUserRating { get; init; }
}

// ─── Review ───────────────────────────────────────────────────────────────────

public class ReviewDto
{
    public int Id { get; init; }
    public UserDto User { get; init; } = null!;
    public MovieDto Movie { get; init; } = null!;
    public string Text { get; init; } = string.Empty;
    public int Rating { get; init; }
    public DateTime CreatedAt { get; init; }
    public int BoostCount { get; init; }
    public List<BoostDto> RecentBoosts { get; init; } = new();
}

public record CreateReviewRequest(int MovieId, string Text, int Rating);

// ─── Boost ────────────────────────────────────────────────────────────────────

public class BoostDto
{
    public int Id { get; init; }
    public UserDto BoosterUser { get; init; } = null!;
    public string? Comment { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record CreateBoostRequest(int ReviewId, string? Comment);

// ─── Feed ─────────────────────────────────────────────────────────────────────

public class FeedItemDto
{
    public string Type { get; init; } = string.Empty;
    public int Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public ReviewDto? Review { get; init; }
    public BoostDto? Boost { get; init; }
}

public class PagedResult<T>
{
    public List<T> Items { get; init; } = new();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }

    public PagedResult() { }

    public PagedResult(List<T> items, int page, int pageSize, int totalCount, int totalPages)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = totalPages;
    }
}

// ─── Pool ─────────────────────────────────────────────────────────────────────

public class PoolDto
{
    public int Id { get; init; }
    public string Platform { get; init; } = string.Empty;
    public UserDto Creator { get; init; } = null!;
    public string Plan { get; init; } = string.Empty;
    public decimal PricePerSlot { get; init; }
    public int TotalSlots { get; init; }
    public int FilledSlots { get; init; }
    public int AvailableSlots { get; init; }
    public string Country { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool IsExpired { get; init; }
    public bool IsFull { get; init; }
}

public record CreatePoolRequest(
    Platform Platform,
    string Plan,
    decimal PricePerSlot,
    int TotalSlots,
    string Country,
    DateTime ExpiresAt
);

// ─── WatchedMovie ─────────────────────────────────────────────────────────────

public class WatchedMovieDto
{
    public int Id { get; init; }
    public MovieDto Movie { get; init; } = null!;
    public int? Rating { get; init; }
    public DateTime WatchedDate { get; init; }
}

public record RateMovieRequest(int? Rating);

// ─── Preferences ─────────────────────────────────────────────────────────────

public class UserPreferencesDto
{
    public Guid UserId { get; init; }
    public List<string> FavoriteGenres { get; init; } = new();
    public List<string> FavoritePlatforms { get; init; } = new();
    public DateTime UpdatedAt { get; init; }
}

public record UpdatePreferencesRequest(
    List<string> FavoriteGenres,
    List<string> FavoritePlatforms
);

// ─── AI Suggestions ───────────────────────────────────────────────────────────

public record SuggestionContextDto(
    Guid UserId,
    string UserName,
    List<string> FavoriteGenres,
    List<string> FavoritePlatforms,
    List<WatchedMovieSummaryDto> WatchHistory,
    List<string> ReviewedGenres,
    double AverageRating
);

public record WatchedMovieSummaryDto(
    int MovieId,
    string Title,
    string? Genre,
    int? UserRating,
    DateTime WatchedDate
);