using CinePool.API.Models;

namespace CinePool.API.Repositories;

// ─── Generic Repository ───────────────────────────────────────────────────────

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(object id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
}

// ─── Specific Repositories ────────────────────────────────────────────────────

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdWithDetailsAsync(Guid id);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
}

public interface IMovieRepository : IRepository<Movie>
{
    Task<IEnumerable<Movie>> SearchAsync(string query, int page, int size);
    Task<Movie?> GetByIdWithDetailsAsync(int id);
    Task<int> CountSearchAsync(string query);
}

public interface IReviewRepository : IRepository<Review>
{
    Task<IEnumerable<Review>> GetFeedAsync(int page, int size);
    Task<int> GetFeedCountAsync();
    Task<Review?> GetByIdWithDetailsAsync(int id);
    Task<bool> ExistsAsync(Guid userId, int movieId);
}

public interface IBoostRepository : IRepository<Boost>
{
    Task<bool> ExistsAsync(Guid userId, int reviewId);
}

public interface IPoolRepository : IRepository<Pool>
{
    Task<IEnumerable<Pool>> GetFilteredAsync(Platform? platform, string? country, int page, int size);
    Task<int> GetFilteredCountAsync(Platform? platform, string? country);
    Task<Pool?> GetByIdWithDetailsAsync(int id);
}

public interface IWatchedMovieRepository : IRepository<WatchedMovie>
{
    Task<IEnumerable<WatchedMovie>> GetByUserAsync(Guid userId, int page, int size);
    Task<int> GetCountByUserAsync(Guid userId);
    Task<WatchedMovie?> GetByUserAndMovieAsync(Guid userId, int movieId);
}

public interface IUserPreferencesRepository : IRepository<UserPreferences>
{
    Task<UserPreferences?> GetByUserIdAsync(Guid userId);
}
public interface IPoolMemberRepository : IRepository<PoolMember>
{
    Task<bool> IsMemberAsync(Guid userId, int poolId);
}

// ─── Unit of Work ─────────────────────────────────────────────────────────────

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IMovieRepository Movies { get; }
    IReviewRepository Reviews { get; }
    IBoostRepository Boosts { get; }
    IPoolRepository Pools { get; }
    IPoolMemberRepository PoolMembers { get; }
    IWatchedMovieRepository WatchedMovies { get; }
    IUserPreferencesRepository UserPreferences { get; }

    Task<int> SaveChangesAsync();
}
