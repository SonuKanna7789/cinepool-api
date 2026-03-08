using CinePool.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CinePool.API.Data.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(CinePoolDbContext context)
    {
        if (await context.Movies.AnyAsync()) return;

        // ── Users ─────────────────────────────────────────────────────────────
        var user1 = new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Alice Cinephile",
            Email = "alice@cinepool.app",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=alice",
            IsEnthusiast = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-3)
        };
        var user2 = new User
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "Bob Moviebuff",
            Email = "bob@cinepool.app",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=bob",
            IsEnthusiast = false,
            CreatedAt = DateTime.UtcNow.AddMonths(-2)
        };
        var user3 = new User
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Name = "Carol Streamlover",
            Email = "carol@cinepool.app",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=carol",
            IsEnthusiast = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-1)
        };
        await context.Users.AddRangeAsync(user1, user2, user3);
        await context.SaveChangesAsync();

        // ── Movies (no explicit Id — let SQL Server auto-increment) ───────────
        var inception = new Movie { Title = "Inception", Year = 2010, Genre = "Sci-Fi/Thriller", Director = "Christopher Nolan", Rating = 8.8, PosterUrl = "https://image.tmdb.org/t/p/w500/9gk7adHYeDvHkCSEqAvQNLV5Uge.jpg" };
        var shawshank = new Movie { Title = "The Shawshank Redemption", Year = 1994, Genre = "Drama", Director = "Frank Darabont", Rating = 9.3, PosterUrl = "https://image.tmdb.org/t/p/w500/q6y0Go1tsGEsmtFryDOJo3dEmqu.jpg" };
        var interstellar = new Movie { Title = "Interstellar", Year = 2014, Genre = "Sci-Fi/Adventure", Director = "Christopher Nolan", Rating = 8.6, PosterUrl = "https://image.tmdb.org/t/p/w500/gEU2QniE6E77NI6lCU6MxlNBvIx.jpg" };
        var parasite = new Movie { Title = "Parasite", Year = 2019, Genre = "Thriller/Drama", Director = "Bong Joon-ho", Rating = 8.5, PosterUrl = "https://image.tmdb.org/t/p/w500/7IiTTgloJzvGI1TAYymCfbfl3vT.jpg" };
        var darkKnight = new Movie { Title = "The Dark Knight", Year = 2008, Genre = "Action/Crime", Director = "Christopher Nolan", Rating = 9.0, PosterUrl = "https://image.tmdb.org/t/p/w500/qJ2tW6WMUDux911r6m7haRef0WH.jpg" };
        var dune2 = new Movie { Title = "Dune: Part Two", Year = 2024, Genre = "Sci-Fi/Adventure", Director = "Denis Villeneuve", Rating = 8.5, PosterUrl = "https://image.tmdb.org/t/p/w500/1pdfLvkbY9ohJlCjQH2CZjjYVvJ.jpg" };
        var oppenheimer = new Movie { Title = "Oppenheimer", Year = 2023, Genre = "Drama/History", Director = "Christopher Nolan", Rating = 8.3, PosterUrl = "https://image.tmdb.org/t/p/w500/8Gxv8gSFCU0XGDykEGv7zR1n2ua.jpg" };
        var poorThings = new Movie { Title = "Poor Things", Year = 2023, Genre = "Fantasy/Drama", Director = "Yorgos Lanthimos", Rating = 7.9, PosterUrl = "https://image.tmdb.org/t/p/w500/kCGlIMHnOm8JPXIbpAlY0ckXWvP.jpg" };
        var eeaao = new Movie { Title = "Everything Everywhere All at Once", Year = 2022, Genre = "Sci-Fi/Comedy", Director = "Daniel Kwan, Daniel Scheinert", Rating = 7.8, PosterUrl = "https://image.tmdb.org/t/p/w500/w3LxiVYdWWRvEVdn5RYq6jIqkb1.jpg" };
        var brutalist = new Movie { Title = "The Brutalist", Year = 2024, Genre = "Drama", Director = "Brady Corbet", Rating = 7.6, PosterUrl = "https://image.tmdb.org/t/p/w500/czIQzgMr7vAXJEFKE7iRjxSXKoZ.jpg" };

        await context.Movies.AddRangeAsync(inception, shawshank, interstellar, parasite, darkKnight, dune2, oppenheimer, poorThings, eeaao, brutalist);
        await context.SaveChangesAsync();
        // IDs are now populated by EF Core after SaveChanges ^^

        // ── MoviePlatforms (use navigation object references, not raw IDs) ────
        await context.MoviePlatforms.AddRangeAsync(
            new MoviePlatform { MovieId = inception.Id, Platform = Platform.Netflix },
            new MoviePlatform { MovieId = inception.Id, Platform = Platform.Prime },
            new MoviePlatform { MovieId = shawshank.Id, Platform = Platform.HBO },
            new MoviePlatform { MovieId = interstellar.Id, Platform = Platform.Prime },
            new MoviePlatform { MovieId = parasite.Id, Platform = Platform.Prime },
            new MoviePlatform { MovieId = parasite.Id, Platform = Platform.Disney },
            new MoviePlatform { MovieId = darkKnight.Id, Platform = Platform.HBO },
            new MoviePlatform { MovieId = darkKnight.Id, Platform = Platform.Netflix },
            new MoviePlatform { MovieId = dune2.Id, Platform = Platform.Prime },
            new MoviePlatform { MovieId = oppenheimer.Id, Platform = Platform.Prime },
            new MoviePlatform { MovieId = poorThings.Id, Platform = Platform.Disney },
            new MoviePlatform { MovieId = eeaao.Id, Platform = Platform.Netflix },
            new MoviePlatform { MovieId = brutalist.Id, Platform = Platform.HBO }
        );
        await context.SaveChangesAsync();

        // ── Reviews ───────────────────────────────────────────────────────────
        var review1 = new Review { UserId = user1.Id, MovieId = inception.Id, Rating = 5, Text = "Mind-bending masterpiece! The layered dream sequences are unlike anything in cinema.", CreatedAt = DateTime.UtcNow.AddDays(-20) };
        var review2 = new Review { UserId = user2.Id, MovieId = darkKnight.Id, Rating = 5, Text = "The Joker's performance alone makes this the greatest superhero film ever made.", CreatedAt = DateTime.UtcNow.AddDays(-15) };
        var review3 = new Review { UserId = user3.Id, MovieId = parasite.Id, Rating = 5, Text = "Bong Joon-ho is a genius. The class critique hits harder on every rewatch.", CreatedAt = DateTime.UtcNow.AddDays(-10) };
        var review4 = new Review { UserId = user1.Id, MovieId = oppenheimer.Id, Rating = 4, Text = "A technical triumph. Cillian Murphy carries the film on his shoulders.", CreatedAt = DateTime.UtcNow.AddDays(-8) };
        var review5 = new Review { UserId = user2.Id, MovieId = eeaao.Id, Rating = 5, Text = "Absolutely unhinged in the best possible way. Pure creative chaos.", CreatedAt = DateTime.UtcNow.AddDays(-5) };
        var review6 = new Review { UserId = user3.Id, MovieId = dune2.Id, Rating = 4, Text = "Villeneuve continues to prove he's the best sci-fi director working today.", CreatedAt = DateTime.UtcNow.AddDays(-3) };

        await context.Reviews.AddRangeAsync(review1, review2, review3, review4, review5, review6);
        await context.SaveChangesAsync();
        // Review IDs now populated ^^

        // ── Boosts ────────────────────────────────────────────────────────────
        await context.Boosts.AddRangeAsync(
            new Boost { ReviewId = review1.Id, BoosterUserId = user2.Id, Comment = "100% agree — this changed how I watch movies!", CreatedAt = DateTime.UtcNow.AddDays(-19) },
            new Boost { ReviewId = review1.Id, BoosterUserId = user3.Id, Comment = "Still thinking about it years later.", CreatedAt = DateTime.UtcNow.AddDays(-18) },
            new Boost { ReviewId = review2.Id, BoosterUserId = user1.Id, Comment = "The Joker rewatch is mandatory.", CreatedAt = DateTime.UtcNow.AddDays(-14) },
            new Boost { ReviewId = review3.Id, BoosterUserId = user1.Id, Comment = "The stairs scene is peak cinema.", CreatedAt = DateTime.UtcNow.AddDays(-9) },
            new Boost { ReviewId = review5.Id, BoosterUserId = user3.Id, Comment = "The hot dog fingers scene broke my brain in the best way.", CreatedAt = DateTime.UtcNow.AddDays(-4) }
        );
        await context.SaveChangesAsync();

        // ── Pools (no explicit Id) ────────────────────────────────────────────
        var pool1 = new Pool { Platform = Platform.Netflix, CreatorUserId = user1.Id, Plan = "Standard with Ads", PricePerSlot = 4.99m, TotalSlots = 4, FilledSlots = 2, Country = "US", ExpiresAt = DateTime.UtcNow.AddMonths(6) };
        var pool2 = new Pool { Platform = Platform.Prime, CreatorUserId = user2.Id, Plan = "Annual Prime", PricePerSlot = 3.50m, TotalSlots = 3, FilledSlots = 1, Country = "AU", ExpiresAt = DateTime.UtcNow.AddMonths(10) };
        var pool3 = new Pool { Platform = Platform.Disney, CreatorUserId = user3.Id, Plan = "Disney Bundle", PricePerSlot = 5.00m, TotalSlots = 4, FilledSlots = 3, Country = "US", ExpiresAt = DateTime.UtcNow.AddMonths(3) };

        await context.Pools.AddRangeAsync(pool1, pool2, pool3);
        await context.SaveChangesAsync();
        // Pool IDs now populated ^^

        // ── Pool Members ──────────────────────────────────────────────────────
        await context.PoolMembers.AddRangeAsync(
            new PoolMember { PoolId = pool1.Id, UserId = user2.Id, JoinedAt = DateTime.UtcNow.AddDays(-30) },
            new PoolMember { PoolId = pool1.Id, UserId = user3.Id, JoinedAt = DateTime.UtcNow.AddDays(-20) },
            new PoolMember { PoolId = pool2.Id, UserId = user1.Id, JoinedAt = DateTime.UtcNow.AddDays(-15) },
            new PoolMember { PoolId = pool3.Id, UserId = user1.Id, JoinedAt = DateTime.UtcNow.AddDays(-10) },
            new PoolMember { PoolId = pool3.Id, UserId = user2.Id, JoinedAt = DateTime.UtcNow.AddDays(-5) }
        );
        await context.SaveChangesAsync();

        // ── Watched Movies ────────────────────────────────────────────────────
        await context.WatchedMovies.AddRangeAsync(
            new WatchedMovie { UserId = user1.Id, MovieId = inception.Id, Rating = 5, WatchedDate = DateTime.UtcNow.AddDays(-25) },
            new WatchedMovie { UserId = user1.Id, MovieId = interstellar.Id, Rating = 4, WatchedDate = DateTime.UtcNow.AddDays(-20) },
            new WatchedMovie { UserId = user1.Id, MovieId = oppenheimer.Id, Rating = 4, WatchedDate = DateTime.UtcNow.AddDays(-10) },
            new WatchedMovie { UserId = user2.Id, MovieId = darkKnight.Id, Rating = 5, WatchedDate = DateTime.UtcNow.AddDays(-18) },
            new WatchedMovie { UserId = user2.Id, MovieId = eeaao.Id, Rating = 5, WatchedDate = DateTime.UtcNow.AddDays(-7) },
            new WatchedMovie { UserId = user3.Id, MovieId = parasite.Id, Rating = 5, WatchedDate = DateTime.UtcNow.AddDays(-12) },
            new WatchedMovie { UserId = user3.Id, MovieId = dune2.Id, Rating = 4, WatchedDate = DateTime.UtcNow.AddDays(-3) }
        );
        await context.SaveChangesAsync();

        // ── User Preferences ──────────────────────────────────────────────────
        await context.UserPreferences.AddRangeAsync(
            new UserPreferences { UserId = user1.Id, FavoriteGenres = """["Sci-Fi","Thriller","Drama"]""", FavoritePlatforms = """["Netflix","Prime"]""", UpdatedAt = DateTime.UtcNow },
            new UserPreferences { UserId = user2.Id, FavoriteGenres = """["Action","Crime","Comedy"]""", FavoritePlatforms = """["HBO","Netflix"]""", UpdatedAt = DateTime.UtcNow },
            new UserPreferences { UserId = user3.Id, FavoriteGenres = """["Drama","Fantasy","Sci-Fi"]""", FavoritePlatforms = """["Disney","Prime"]""", UpdatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();
    }
}