# 🎬 CinePool API

> Social movie discovery + OTT subscription pooling — .NET 8 Web API

---

## 📦 Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core 8 Web API |
| ORM | Entity Framework Core 8 (Code-First) |
| Database | SQL Server |
| Auth | JWT Bearer + Refresh Tokens |
| Validation | FluentValidation |
| Mapping | AutoMapper |
| Docs | Swagger / OpenAPI |
| Pattern | Repository + Unit of Work |

---

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB / Express / Full / Azure SQL / Docker)

### 1. Clone & Configure

```bash
git clone <repo>
cd CinePool.API
```

Edit `appsettings.json` (or set env vars):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CinePoolDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Secret": "YourSuperSecretKeyMustBe32CharsOrMore!",
    "Issuer": "CinePool.API",
    "Audience": "CinePool.Clients",
    "ExpiryMinutes": "60"
  }
}
```

### 2. Run (migrations + seeding happen automatically on startup)

```bash
dotnet run
```

The API will be available at:
- **https://localhost:7xxx** (HTTPS)
- **http://localhost:5xxx** (HTTP)
- **Swagger UI:** https://localhost:7xxx/swagger

### 3. Manual Migrations (optional — auto-applied on startup)

```bash
dotnet ef database update
```

---

## 🗄️ Database Schema

```
Users ──────────────┐
  │                 │
  ├── Reviews ──── Boosts
  │     └── Movie
  │
  ├── Pools ──── PoolMembers
  │
  ├── WatchedMovies ── Movie
  │
  └── UserPreferences

Movies ── MoviePlatforms
```

---

## 🔑 Authentication

CinePool uses **JWT Bearer tokens** with refresh token rotation.

### Flow
1. `POST /api/auth/register` or `POST /api/auth/login`
2. Receive `{ accessToken, refreshToken, user }`
3. Include `Authorization: Bearer <accessToken>` on all protected requests
4. When access token expires, call `POST /api/auth/refresh` with the refresh token

### Seed Credentials
| Email | Password |
|-------|----------|
| alice@cinepool.app | Password123! |
| bob@cinepool.app | Password123! |
| carol@cinepool.app | Password123! |

---

## 📡 API Reference

### Auth
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | — | Register new user |
| POST | `/api/auth/login` | — | Login, get tokens |
| POST | `/api/auth/refresh` | — | Refresh access token |

### Feed
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/feed?page=&size=` | — | Paginated social feed |

### Movies
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/movies/search?q=` | — | Search movies |
| GET | `/api/movies/{id}` | — | Movie details |
| POST | `/api/movies/{id}/rate` | ✅ | Rate / mark as watched |

### Reviews & Boosts
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/reviews` | ✅ | Create review |
| POST | `/api/boosts` | ✅ | Boost a review |

### Pools
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/pools?platform=&country=` | — | List pools |
| POST | `/api/pools` | ✅ | Create pool |
| POST | `/api/pools/{id}/join` | ✅ | Join pool |

### Profile
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/profile` | ✅ | User profile + stats |
| GET | `/api/profile/watched` | ✅ | Watched movies |
| GET | `/api/profile/preferences` | ✅ | User preferences |
| PUT | `/api/profile/preferences` | ✅ | Update preferences |

### AI Suggestions
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/suggestions` | ✅ | AI-ready context payload |

---

## 🤖 AI Suggestion Context

`GET /api/suggestions` returns a rich payload for your AI service:

```json
{
  "userId": "uuid",
  "userName": "Alice",
  "favoriteGenres": ["Sci-Fi", "Thriller"],
  "favoritePlatforms": ["Netflix", "Prime"],
  "watchHistory": [
    { "movieId": 1, "title": "Inception", "genre": "Sci-Fi", "userRating": 5, "watchedDate": "..." }
  ],
  "reviewedGenres": ["Sci-Fi", "Drama"],
  "averageRating": 4.5
}
```

Pass this to your LLM with a prompt like:
> *"Based on this user's watch history and preferences, suggest 5 movies they haven't seen yet..."*

---

## 🌐 CORS

Allowed origins (configured in `Program.cs`):
- `https://cinepool-palace.lovable.app`
- `http://localhost:5173`
- `http://localhost:3000`

---

## 🐳 Docker

```bash
docker build -t cinepool-api .
docker run -p 8080:8080 \
  -e "ConnectionStrings__DefaultConnection=Server=host.docker.internal;..." \
  -e "Jwt__Secret=YourSecretHere" \
  cinepool-api
```

---

## 📁 Project Structure

```
CinePool.API/
├── Controllers/
│   ├── AuthController.cs
│   ├── FeedController.cs
│   ├── MoviesController.cs
│   ├── ReviewsController.cs   (+ BoostsController)
│   ├── PoolsController.cs
│   └── ProfileController.cs  (+ SuggestionsController)
├── DTOs/
│   ├── Dtos.cs                ← All request/response DTOs
│   └── Validators.cs          ← FluentValidation rules
├── Models/
│   └── Entities.cs            ← All EF Core entities + Platform enum
├── Data/
│   ├── CinePoolDbContext.cs
│   ├── Migrations/
│   └── Seed/
│       └── DatabaseSeeder.cs  ← 10 movies, 3 users, reviews, pools
├── Services/
│   └── Services.cs            ← Auth, Feed, Review, Movie, Pool, Profile
├── Repositories/
│   ├── IRepositories.cs       ← Interfaces + IUnitOfWork
│   └── Repositories.cs        ← Implementations
├── Mappings/
│   └── MappingProfile.cs      ← AutoMapper profiles
├── Middleware/
│   └── GlobalExceptionMiddleware.cs
├── Auth/
│   └── JwtService.cs
├── Program.cs
├── appsettings.json
├── Dockerfile
└── README.md
```

---

## ⚙️ Environment Variables (Production)

| Variable | Description |
|----------|-------------|
| `ConnectionStrings__DefaultConnection` | SQL Server connection string |
| `Jwt__Secret` | JWT signing secret (≥32 chars) |
| `Jwt__Issuer` | Token issuer |
| `Jwt__Audience` | Token audience |
| `Jwt__ExpiryMinutes` | Access token TTL in minutes |
| `ASPNETCORE_ENVIRONMENT` | `Production` / `Development` |
