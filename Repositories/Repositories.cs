using CinePool.API.Data;
using CinePool.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CinePool.API.Repositories;

// ─── Generic Repository ───────────────────────────────────────────────────────

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly CinePoolDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(CinePoolDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(object id) => await _dbSet.FindAsync(id);
    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();
    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
    public void Update(T entity) => _dbSet.Update(entity);
    public void Remove(T entity) => _dbSet.Remove(entity);
}

// ─── User Repository ──────────────────────────────────────────────────────────

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(CinePoolDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email) =>
        await _dbSet.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

    public async Task<User?> GetByIdWithDetailsAsync(Guid id) =>
        await _dbSet
            .Include(u => u.Reviews)
            .Include(u => u.WatchedMovies)
            .Include(u => u.Boosts)
            .Include(u => u.CreatedPools)
            .Include(u => u.Preferences)
            .FirstOrDefaultAsync(u => u.Id == id);

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken) =>
        await _dbSet.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
}

// ─── Movie Repository ─────────────────────────────────────────────────────────

public class MovieRepository : Repository<Movie>, IMovieRepository
{
    public MovieRepository(CinePoolDbContext context) : base(context) { }

    public async Task<IEnumerable<Movie>> SearchAsync(string query, int page, int size)
    {
        var q = _dbSet
            .Include(m => m.Platforms)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var lower = query.ToLower();
            q = q.Where(m =>
                m.Title.ToLower().Contains(lower) ||
                (m.Director != null && m.Director.ToLower().Contains(lower)) ||
                (m.Genre != null && m.Genre.ToLower().Contains(lower)));
        }

        return await q
            .OrderByDescending(m => m.Rating)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();
    }

    public async Task<int> CountSearchAsync(string query)
    {
        var q = _dbSet.AsQueryable();
        if (!string.IsNullOrWhiteSpace(query))
        {
            var lower = query.ToLower();
            q = q.Where(m =>
                m.Title.ToLower().Contains(lower) ||
                (m.Director != null && m.Director.ToLower().Contains(lower)) ||
                (m.Genre != null && m.Genre.ToLower().Contains(lower)));
        }
        return await q.CountAsync();
    }

    public async Task<Movie?> GetByIdWithDetailsAsync(int id) =>
        await _dbSet
            .Include(m => m.Platforms)
            .Include(m => m.Reviews)
                .ThenInclude(r => r.User)
            .Include(m => m.Reviews)
                .ThenInclude(r => r.Boosts)
                    .ThenInclude(b => b.BoosterUser)
            .FirstOrDefaultAsync(m => m.Id == id);
}

// ─── Review Repository ────────────────────────────────────────────────────────

public class ReviewRepository : Repository<Review>, IReviewRepository
{
    public ReviewRepository(CinePoolDbContext context) : base(context) { }

    public async Task<IEnumerable<Review>> GetFeedAsync(int page, int size) =>
        await _dbSet
            .Include(r => r.User)
            .Include(r => r.Movie).ThenInclude(m => m.Platforms)
            .Include(r => r.Boosts).ThenInclude(b => b.BoosterUser)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

    public async Task<int> GetFeedCountAsync() => await _dbSet.CountAsync();

    public async Task<Review?> GetByIdWithDetailsAsync(int id) =>
        await _dbSet
            .Include(r => r.User)
            .Include(r => r.Movie).ThenInclude(m => m.Platforms)
            .Include(r => r.Boosts).ThenInclude(b => b.BoosterUser)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<bool> ExistsAsync(Guid userId, int movieId) =>
        await _dbSet.AnyAsync(r => r.UserId == userId && r.MovieId == movieId);
}

// ─── Boost Repository ─────────────────────────────────────────────────────────

public class BoostRepository : Repository<Boost>, IBoostRepository
{
    public BoostRepository(CinePoolDbContext context) : base(context) { }

    public async Task<bool> ExistsAsync(Guid userId, int reviewId) =>
        await _dbSet.AnyAsync(b => b.BoosterUserId == userId && b.ReviewId == reviewId);
}

// ─── Pool Repository ──────────────────────────────────────────────────────────

public class PoolRepository : Repository<Pool>, IPoolRepository
{
    public PoolRepository(CinePoolDbContext context) : base(context) { }

    public async Task<IEnumerable<Pool>> GetFilteredAsync(Platform? platform, string? country, int page, int size)
    {
        var q = _dbSet
            .Include(p => p.Creator)
            .Include(p => p.Members)
            .AsQueryable();

        if (platform.HasValue)
            q = q.Where(p => p.Platform == platform.Value);

        if (!string.IsNullOrWhiteSpace(country))
            q = q.Where(p => p.Country.ToLower() == country.ToLower());

        return await q
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();
    }

    public async Task<int> GetFilteredCountAsync(Platform? platform, string? country)
    {
        var q = _dbSet.AsQueryable();
        if (platform.HasValue) q = q.Where(p => p.Platform == platform.Value);
        if (!string.IsNullOrWhiteSpace(country)) q = q.Where(p => p.Country.ToLower() == country.ToLower());
        return await q.CountAsync();
    }

    public async Task<Pool?> GetByIdWithDetailsAsync(int id) =>
        await _dbSet
            .Include(p => p.Creator)
            .Include(p => p.Members).ThenInclude(m => m.User)
            .FirstOrDefaultAsync(p => p.Id == id);
}

// ─── WatchedMovie Repository ──────────────────────────────────────────────────

public class WatchedMovieRepository : Repository<WatchedMovie>, IWatchedMovieRepository
{
    public WatchedMovieRepository(CinePoolDbContext context) : base(context) { }

    public async Task<IEnumerable<WatchedMovie>> GetByUserAsync(Guid userId, int page, int size) =>
        await _dbSet
            .Include(wm => wm.Movie).ThenInclude(m => m.Platforms)
            .Where(wm => wm.UserId == userId)
            .OrderByDescending(wm => wm.WatchedDate)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

    public async Task<int> GetCountByUserAsync(Guid userId) =>
        await _dbSet.CountAsync(wm => wm.UserId == userId);

    public async Task<WatchedMovie?> GetByUserAndMovieAsync(Guid userId, int movieId) =>
        await _dbSet.FirstOrDefaultAsync(wm => wm.UserId == userId && wm.MovieId == movieId);
}

// ─── PoolMember Repository ────────────────────────────────────────────────────

public class PoolMemberRepository : Repository<PoolMember>, IPoolMemberRepository
{
    public PoolMemberRepository(CinePoolDbContext context) : base(context) { }

    public async Task<bool> IsMemberAsync(Guid userId, int poolId) =>
        await _dbSet.AnyAsync(pm => pm.UserId == userId && pm.PoolId == poolId);
}

// ─── UserPreferences Repository ───────────────────────────────────────────────

public class UserPreferencesRepository : Repository<UserPreferences>, IUserPreferencesRepository
{
    public UserPreferencesRepository(CinePoolDbContext context) : base(context) { }

    public async Task<UserPreferences?> GetByUserIdAsync(Guid userId) =>
        await _dbSet.FirstOrDefaultAsync(p => p.UserId == userId);
}

// ─── Unit of Work ─────────────────────────────────────────────────────────────

public class UnitOfWork : IUnitOfWork
{
    private readonly CinePoolDbContext _context;

    public IUserRepository Users { get; }
    public IMovieRepository Movies { get; }
    public IReviewRepository Reviews { get; }
    public IBoostRepository Boosts { get; }
    public IPoolRepository Pools { get; }
    public IPoolMemberRepository PoolMembers { get; }
    public IWatchedMovieRepository WatchedMovies { get; }
    public IUserPreferencesRepository UserPreferences { get; }

    public UnitOfWork(CinePoolDbContext context)
    {
        _context = context;
        Users = new UserRepository(context);
        Movies = new MovieRepository(context);
        Reviews = new ReviewRepository(context);
        Boosts = new BoostRepository(context);
        Pools = new PoolRepository(context);
        PoolMembers = new PoolMemberRepository(context);
        WatchedMovies = new WatchedMovieRepository(context);
        UserPreferences = new UserPreferencesRepository(context);
    }

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public void Dispose() => _context.Dispose();
}