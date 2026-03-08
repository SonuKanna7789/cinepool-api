using AutoMapper;
using CinePool.API.Auth;
using CinePool.API.DTOs;
using CinePool.API.Models;
using CinePool.API.Repositories;

namespace CinePool.API.Services;

// ─── Auth Service ─────────────────────────────────────────────────────────────

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshAsync(string refreshToken);
}

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IJwtService _jwt;
    private readonly IMapper _mapper;

    public AuthService(IUnitOfWork uow, IJwtService jwt, IMapper mapper)
    {
        _uow = uow;
        _jwt = jwt;
        _mapper = mapper;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existing = await _uow.Users.GetByEmailAsync(request.Email);
        if (existing != null)
            throw new InvalidOperationException("Email already in use.");

        var user = new User
        {
            Name = request.Name,
            Email = request.Email.ToLower().Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
        };

        var refreshToken = _jwt.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);

        await _uow.Users.AddAsync(user);

        // Create default preferences
        await _uow.UserPreferences.AddAsync(new UserPreferences { UserId = user.Id });

        await _uow.SaveChangesAsync();

        return new AuthResponse(
            _jwt.GenerateAccessToken(user),
            refreshToken,
            _mapper.Map<UserDto>(user)
        );
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _uow.Users.GetByEmailAsync(request.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var refreshToken = _jwt.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();

        return new AuthResponse(
            _jwt.GenerateAccessToken(user),
            refreshToken,
            _mapper.Map<UserDto>(user)
        );
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken)
    {
        var user = await _uow.Users.GetByRefreshTokenAsync(refreshToken);
        if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        var newRefreshToken = _jwt.GenerateRefreshToken();
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();

        return new AuthResponse(
            _jwt.GenerateAccessToken(user),
            newRefreshToken,
            _mapper.Map<UserDto>(user)
        );
    }
}

// ─── Feed Service ─────────────────────────────────────────────────────────────

public interface IFeedService
{
    Task<PagedResult<FeedItemDto>> GetFeedAsync(int page, int size);
}

public class FeedService : IFeedService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public FeedService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PagedResult<FeedItemDto>> GetFeedAsync(int page, int size)
    {
        var reviews = await _uow.Reviews.GetFeedAsync(page, size);
        var total = await _uow.Reviews.GetFeedCountAsync();

        var items = reviews.Select(r => new FeedItemDto
        {
            Type = "review",
            Id = r.Id,
            CreatedAt = r.CreatedAt,
            Review = _mapper.Map<ReviewDto>(r),
            Boost = null
        }).ToList();

        return new PagedResult<FeedItemDto>(
            items,
            page,
            size,
            total,
            (int)Math.Ceiling((double)total / size)
        );
    }
}

// ─── Review Service ───────────────────────────────────────────────────────────

public interface IReviewService
{
    Task<ReviewDto> CreateAsync(Guid userId, CreateReviewRequest request);
    Task<BoostDto> BoostAsync(Guid boosterUserId, CreateBoostRequest request);
}

public class ReviewService : IReviewService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ReviewService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<ReviewDto> CreateAsync(Guid userId, CreateReviewRequest request)
    {
        var movie = await _uow.Movies.GetByIdAsync(request.MovieId)
            ?? throw new KeyNotFoundException($"Movie {request.MovieId} not found.");

        if (await _uow.Reviews.ExistsAsync(userId, request.MovieId))
            throw new InvalidOperationException("You have already reviewed this movie.");

        var review = new Review
        {
            UserId = userId,
            MovieId = request.MovieId,
            Text = request.Text,
            Rating = request.Rating
        };

        await _uow.Reviews.AddAsync(review);
        await _uow.SaveChangesAsync();

        var full = await _uow.Reviews.GetByIdWithDetailsAsync(review.Id)
            ?? throw new InvalidOperationException("Failed to load review.");

        return _mapper.Map<ReviewDto>(full);
    }

    public async Task<BoostDto> BoostAsync(Guid boosterUserId, CreateBoostRequest request)
    {
        var review = await _uow.Reviews.GetByIdAsync(request.ReviewId)
            ?? throw new KeyNotFoundException($"Review {request.ReviewId} not found.");

        if (review.UserId == boosterUserId)
            throw new InvalidOperationException("You cannot boost your own review.");

        if (await _uow.Boosts.ExistsAsync(boosterUserId, request.ReviewId))
            throw new InvalidOperationException("You have already boosted this review.");

        var boost = new Boost
        {
            ReviewId = request.ReviewId,
            BoosterUserId = boosterUserId,
            Comment = request.Comment
        };

        await _uow.Boosts.AddAsync(boost);
        await _uow.SaveChangesAsync();

        // Reload with user
        var user = await _uow.Users.GetByIdAsync(boosterUserId)
            ?? throw new InvalidOperationException("User not found.");

        return new BoostDto
        {
            Id = boost.Id,
            BoosterUser = _mapper.Map<UserDto>(user),
            Comment = boost.Comment,
            CreatedAt = boost.CreatedAt
        };
    }
}

// ─── Movie Service ────────────────────────────────────────────────────────────

public interface IMovieService
{
    Task<PagedResult<MovieDto>> SearchAsync(string? q, int page, int size);
    Task<MovieDetailDto> GetByIdAsync(int id);
    Task<WatchedMovieDto> RateOrMarkWatchedAsync(Guid userId, int movieId, RateMovieRequest request);
}

public class MovieService : IMovieService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public MovieService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PagedResult<MovieDto>> SearchAsync(string? q, int page, int size)
    {
        var movies = await _uow.Movies.SearchAsync(q ?? "", page, size);
        var total = await _uow.Movies.CountSearchAsync(q ?? "");

        return new PagedResult<MovieDto>(
            movies.Select(m => _mapper.Map<MovieDto>(m)).ToList(),
            page, size, total,
            (int)Math.Ceiling((double)total / size)
        );
    }

    public async Task<MovieDetailDto> GetByIdAsync(int id)
    {
        var movie = await _uow.Movies.GetByIdWithDetailsAsync(id)
            ?? throw new KeyNotFoundException($"Movie {id} not found.");

        var mapped = _mapper.Map<MovieDetailDto>(movie);

        var dto = new MovieDetailDto
        {
            Id = mapped.Id,
            Title = mapped.Title,
            Year = mapped.Year,
            PosterUrl = mapped.PosterUrl,
            Genre = mapped.Genre,
            Director = mapped.Director,
            Rating = mapped.Rating,
            CreatedAt = mapped.CreatedAt,
            Platforms = mapped.Platforms,
            TotalReviews = movie.Reviews.Count,
            AverageUserRating = movie.Reviews.Any() ? movie.Reviews.Average(r => r.Rating) : 0,
            RecentReviews = movie.Reviews
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .Select(r => _mapper.Map<ReviewDto>(r))
                .ToList()
        };

        return dto;
    }

    public async Task<WatchedMovieDto> RateOrMarkWatchedAsync(Guid userId, int movieId, RateMovieRequest request)
    {
        var movie = await _uow.Movies.GetByIdAsync(movieId)
            ?? throw new KeyNotFoundException($"Movie {movieId} not found.");

        var existing = await _uow.WatchedMovies.GetByUserAndMovieAsync(userId, movieId);

        if (existing != null)
        {
            existing.Rating = request.Rating;
            existing.WatchedDate = DateTime.UtcNow;
            _uow.WatchedMovies.Update(existing);
        }
        else
        {
            existing = new WatchedMovie
            {
                UserId = userId,
                MovieId = movieId,
                Rating = request.Rating,
                WatchedDate = DateTime.UtcNow
            };
            await _uow.WatchedMovies.AddAsync(existing);
        }

        await _uow.SaveChangesAsync();

        return new WatchedMovieDto
        {
            Id = existing.Id,
            Movie = _mapper.Map<MovieDto>(movie),
            Rating = existing.Rating,
            WatchedDate = existing.WatchedDate
        };
    }
}

// ─── Pool Service ─────────────────────────────────────────────────────────────

public interface IPoolService
{
    Task<PagedResult<PoolDto>> GetPoolsAsync(Platform? platform, string? country, int page, int size);
    Task<PoolDto> CreatePoolAsync(Guid creatorId, CreatePoolRequest request);
    Task<PoolDto> JoinPoolAsync(Guid userId, int poolId);
}

public class PoolService : IPoolService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public PoolService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PagedResult<PoolDto>> GetPoolsAsync(Platform? platform, string? country, int page, int size)
    {
        var pools = await _uow.Pools.GetFilteredAsync(platform, country, page, size);
        var total = await _uow.Pools.GetFilteredCountAsync(platform, country);

        return new PagedResult<PoolDto>(
            pools.Select(p => _mapper.Map<PoolDto>(p)).ToList(),
            page, size, total,
            (int)Math.Ceiling((double)total / size)
        );
    }

    public async Task<PoolDto> CreatePoolAsync(Guid creatorId, CreatePoolRequest request)
    {
        var pool = new Pool
        {
            Platform = request.Platform,
            CreatorUserId = creatorId,
            Plan = request.Plan,
            PricePerSlot = request.PricePerSlot,
            TotalSlots = request.TotalSlots,
            FilledSlots = 1, // creator counts as first member
            Country = request.Country,
            ExpiresAt = request.ExpiresAt
        };

        await _uow.Pools.AddAsync(pool);
        await _uow.SaveChangesAsync();

        // Creator auto-joins
        await _uow.PoolMembers.AddAsync(new PoolMember
        {
            PoolId = pool.Id,
            UserId = creatorId,
            JoinedAt = DateTime.UtcNow
        });
        await _uow.SaveChangesAsync();

        var full = await _uow.Pools.GetByIdWithDetailsAsync(pool.Id)!;
        return _mapper.Map<PoolDto>(full!);
    }

    public async Task<PoolDto> JoinPoolAsync(Guid userId, int poolId)
    {
        var pool = await _uow.Pools.GetByIdWithDetailsAsync(poolId)
            ?? throw new KeyNotFoundException($"Pool {poolId} not found.");

        if (pool.ExpiresAt < DateTime.UtcNow)
            throw new InvalidOperationException("This pool has expired.");

        if (pool.FilledSlots >= pool.TotalSlots)
            throw new InvalidOperationException("This pool is already full.");

        if (pool.Members.Any(m => m.UserId == userId))
            throw new InvalidOperationException("You are already a member of this pool.");

        await _uow.PoolMembers.AddAsync(new PoolMember
        {
            PoolId = poolId,
            UserId = userId,
            JoinedAt = DateTime.UtcNow
        });

        pool.FilledSlots++;
        _uow.Pools.Update(pool);
        await _uow.SaveChangesAsync();

        var updated = await _uow.Pools.GetByIdWithDetailsAsync(poolId)!;
        return _mapper.Map<PoolDto>(updated!);
    }
}

// ─── Profile Service ──────────────────────────────────────────────────────────

public interface IProfileService
{
    Task<ProfileDto> GetProfileAsync(Guid userId);
    Task<PagedResult<WatchedMovieDto>> GetWatchedAsync(Guid userId, int page, int size);
    Task<UserPreferencesDto> GetPreferencesAsync(Guid userId);
    Task<UserPreferencesDto> UpdatePreferencesAsync(Guid userId, UpdatePreferencesRequest request);
    Task<SuggestionContextDto> GetSuggestionContextAsync(Guid userId);
}

public class ProfileService : IProfileService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ProfileService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<ProfileDto> GetProfileAsync(Guid userId)
    {
        var user = await _uow.Users.GetByIdWithDetailsAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        return new ProfileDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Avatar = user.Avatar,
            IsEnthusiast = user.IsEnthusiast,
            CreatedAt = user.CreatedAt,
            ReviewCount = user.Reviews.Count,
            WatchedCount = user.WatchedMovies.Count,
            BoostCount = user.Boosts.Count,
            PoolCount = user.CreatedPools.Count
        };
    }

    public async Task<PagedResult<WatchedMovieDto>> GetWatchedAsync(Guid userId, int page, int size)
    {
        var watched = await _uow.WatchedMovies.GetByUserAsync(userId, page, size);
        var total = await _uow.WatchedMovies.GetCountByUserAsync(userId);

        return new PagedResult<WatchedMovieDto>(
            watched.Select(wm => _mapper.Map<WatchedMovieDto>(wm)).ToList(),
            page, size, total,
            (int)Math.Ceiling((double)total / size)
        );
    }

    public async Task<UserPreferencesDto> GetPreferencesAsync(Guid userId)
    {
        var prefs = await _uow.UserPreferences.GetByUserIdAsync(userId);
        if (prefs == null)
        {
            prefs = new UserPreferences { UserId = userId };
            await _uow.UserPreferences.AddAsync(prefs);
            await _uow.SaveChangesAsync();
        }
        return _mapper.Map<UserPreferencesDto>(prefs);
    }

    public async Task<UserPreferencesDto> UpdatePreferencesAsync(Guid userId, UpdatePreferencesRequest request)
    {
        var prefs = await _uow.UserPreferences.GetByUserIdAsync(userId);
        if (prefs == null)
        {
            prefs = new UserPreferences { UserId = userId };
            await _uow.UserPreferences.AddAsync(prefs);
        }

        prefs.FavoriteGenresList = request.FavoriteGenres;
        prefs.FavoritePlatformsList = request.FavoritePlatforms;
        prefs.UpdatedAt = DateTime.UtcNow;

        _uow.UserPreferences.Update(prefs);
        await _uow.SaveChangesAsync();

        return _mapper.Map<UserPreferencesDto>(prefs);
    }

    public async Task<SuggestionContextDto> GetSuggestionContextAsync(Guid userId)
    {
        var user = await _uow.Users.GetByIdWithDetailsAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var prefs = await _uow.UserPreferences.GetByUserIdAsync(userId);
        var watched = await _uow.WatchedMovies.GetByUserAsync(userId, 1, 50);

        var watchHistory = watched.Select(wm => new WatchedMovieSummaryDto(
            wm.MovieId,
            wm.Movie.Title,
            wm.Movie.Genre,
            wm.Rating,
            wm.WatchedDate
        )).ToList();

        var reviewedGenres = user.Reviews
            .Select(r => r.Movie?.Genre)
            .Where(g => g != null)
            .Distinct()
            .ToList();

        var avgRating = watched.Where(w => w.Rating.HasValue).Any()
            ? watched.Where(w => w.Rating.HasValue).Average(w => (double)w.Rating!.Value)
            : 0;

        return new SuggestionContextDto(
            user.Id,
            user.Name,
            FavoriteGenres: prefs?.FavoriteGenresList ?? new(),
            FavoritePlatforms: prefs?.FavoritePlatformsList ?? new(),
            WatchHistory: watchHistory,
            ReviewedGenres: reviewedGenres!,
            AverageRating: Math.Round(avgRating, 2)
        );
    }
}