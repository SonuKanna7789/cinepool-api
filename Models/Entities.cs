using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace CinePool.API.Models;

// ─── Enums ────────────────────────────────────────────────────────────────────

public enum Platform
{
    Netflix,
    Prime,
    Disney,
    HBO
}

// ─── User ─────────────────────────────────────────────────────────────────────

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public string? Avatar { get; set; }
    public bool IsEnthusiast { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Refresh token support
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

    // Navigation
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Boost> Boosts { get; set; } = new List<Boost>();
    public ICollection<Pool> CreatedPools { get; set; } = new List<Pool>();
    public ICollection<PoolMember> PoolMemberships { get; set; } = new List<PoolMember>();
    public ICollection<WatchedMovie> WatchedMovies { get; set; } = new List<WatchedMovie>();
    public UserPreferences? Preferences { get; set; }
}

// ─── Movie ────────────────────────────────────────────────────────────────────

public class Movie
{
    public int Id { get; set; }

    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    public int Year { get; set; }
    public string? PosterUrl { get; set; }

    [MaxLength(100)]
    public string? Genre { get; set; }

    [MaxLength(200)]
    public string? Director { get; set; }

    public double Rating { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<MoviePlatform> Platforms { get; set; } = new List<MoviePlatform>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<WatchedMovie> WatchedMovies { get; set; } = new List<WatchedMovie>();
}

// ─── MoviePlatform ────────────────────────────────────────────────────────────

public class MoviePlatform
{
    public int Id { get; set; }

    [ForeignKey(nameof(Movie))]
    public int MovieId { get; set; }
    public Movie Movie { get; set; } = null!;

    public Platform Platform { get; set; }
}

// ─── Review ───────────────────────────────────────────────────────────────────

public class Review
{
    public int Id { get; set; }

    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    [ForeignKey(nameof(Movie))]
    public int MovieId { get; set; }
    public Movie Movie { get; set; } = null!;

    [MaxLength(2000)]
    public string Text { get; set; } = string.Empty;

    [Range(1, 5)]
    public int Rating { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Boost> Boosts { get; set; } = new List<Boost>();
}

// ─── Boost ────────────────────────────────────────────────────────────────────

public class Boost
{
    public int Id { get; set; }

    [ForeignKey(nameof(Review))]
    public int ReviewId { get; set; }
    public Review Review { get; set; } = null!;

    [ForeignKey(nameof(User))]
    public Guid BoosterUserId { get; set; }
    public User BoosterUser { get; set; } = null!;

    [MaxLength(500)]
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// ─── Pool ─────────────────────────────────────────────────────────────────────

public class Pool
{
    public int Id { get; set; }
    public Platform Platform { get; set; }

    [ForeignKey(nameof(User))]
    public Guid CreatorUserId { get; set; }
    public User Creator { get; set; } = null!;

    [MaxLength(200)]
    public string Plan { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    public decimal PricePerSlot { get; set; }

    public int TotalSlots { get; set; }
    public int FilledSlots { get; set; } = 0;

    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<PoolMember> Members { get; set; } = new List<PoolMember>();
}

// ─── PoolMember ───────────────────────────────────────────────────────────────

public class PoolMember
{
    public int Id { get; set; }

    [ForeignKey(nameof(Pool))]
    public int PoolId { get; set; }
    public Pool Pool { get; set; } = null!;

    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

// ─── WatchedMovie ─────────────────────────────────────────────────────────────

public class WatchedMovie
{
    public int Id { get; set; }

    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    [ForeignKey(nameof(Movie))]
    public int MovieId { get; set; }
    public Movie Movie { get; set; } = null!;

    [Range(1, 5)]
    public int? Rating { get; set; }

    public DateTime WatchedDate { get; set; } = DateTime.UtcNow;
}

// ─── UserPreferences ──────────────────────────────────────────────────────────

public class UserPreferences
{
    public int Id { get; set; }

    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Stored as JSON string in DB
    public string FavoriteGenres { get; set; } = "[]";
    public string FavoritePlatforms { get; set; } = "[]";

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public List<string> FavoriteGenresList
    {
        get => JsonSerializer.Deserialize<List<string>>(FavoriteGenres) ?? new();
        set => FavoriteGenres = JsonSerializer.Serialize(value);
    }

    [NotMapped]
    public List<string> FavoritePlatformsList
    {
        get => JsonSerializer.Deserialize<List<string>>(FavoritePlatforms) ?? new();
        set => FavoritePlatforms = JsonSerializer.Serialize(value);
    }
}
