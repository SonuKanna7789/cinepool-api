using CinePool.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CinePool.API.Data;

public class CinePoolDbContext : DbContext
{
    public CinePoolDbContext(DbContextOptions<CinePoolDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<MoviePlatform> MoviePlatforms => Set<MoviePlatform>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Boost> Boosts => Set<Boost>();
    public DbSet<Pool> Pools => Set<Pool>();
    public DbSet<PoolMember> PoolMembers => Set<PoolMember>();
    public DbSet<WatchedMovie> WatchedMovies => Set<WatchedMovie>();
    public DbSet<UserPreferences> UserPreferences => Set<UserPreferences>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── User ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
        });

        // ── Movie ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Movie>(e =>
        {
            e.HasKey(m => m.Id);
            e.HasMany(m => m.Platforms)
             .WithOne(mp => mp.Movie)
             .HasForeignKey(mp => mp.MovieId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Review ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Review>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasOne(r => r.User)
             .WithMany(u => u.Reviews)
             .HasForeignKey(r => r.UserId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Movie)
             .WithMany(m => m.Reviews)
             .HasForeignKey(r => r.MovieId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Boost ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Boost>(e =>
        {
            e.HasKey(b => b.Id);
            e.HasOne(b => b.Review)
             .WithMany(r => r.Boosts)
             .HasForeignKey(b => b.ReviewId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(b => b.BoosterUser)
             .WithMany(u => u.Boosts)
             .HasForeignKey(b => b.BoosterUserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Pool ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<Pool>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasOne(p => p.Creator)
             .WithMany(u => u.CreatedPools)
             .HasForeignKey(p => p.CreatorUserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── PoolMember ────────────────────────────────────────────────────────
        modelBuilder.Entity<PoolMember>(e =>
        {
            e.HasKey(pm => pm.Id);
            e.HasIndex(pm => new { pm.PoolId, pm.UserId }).IsUnique();
            e.HasOne(pm => pm.Pool)
             .WithMany(p => p.Members)
             .HasForeignKey(pm => pm.PoolId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(pm => pm.User)
             .WithMany(u => u.PoolMemberships)
             .HasForeignKey(pm => pm.UserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── WatchedMovie ──────────────────────────────────────────────────────
        modelBuilder.Entity<WatchedMovie>(e =>
        {
            e.HasKey(wm => wm.Id);
            e.HasIndex(wm => new { wm.UserId, wm.MovieId }).IsUnique();
            e.HasOne(wm => wm.User)
             .WithMany(u => u.WatchedMovies)
             .HasForeignKey(wm => wm.UserId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(wm => wm.Movie)
             .WithMany(m => m.WatchedMovies)
             .HasForeignKey(wm => wm.MovieId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── UserPreferences ───────────────────────────────────────────────────
        modelBuilder.Entity<UserPreferences>(e =>
        {
            e.HasKey(up => up.Id);
            e.HasIndex(up => up.UserId).IsUnique();
            e.HasOne(up => up.User)
             .WithOne(u => u.Preferences)
             .HasForeignKey<UserPreferences>(up => up.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
