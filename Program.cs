using System.Text;
using CinePool.API.Auth;
using CinePool.API.Data;
using CinePool.API.Data.Seed;
using CinePool.API.DTOs.Validators;
using CinePool.API.Mappings;
using CinePool.API.Middleware;
using CinePool.API.Repositories;
using CinePool.API.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ─── Database ─────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<CinePoolDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null)
    )
);

// ─── CORS ─────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("LovableApp", policy =>
    {
        policy
            //.WithOrigins(
            //    "https://cinepool-palace.lovable.app",
            //    "http://localhost:5173",
            //    "http://localhost:3000"
            //)
            .WithOrigins(
    "https://cinepool-palace.lovable.app",
    "https://68123838-66c6-4e6a-82aa-7801aa444a83.lovableproject.com",
    "https://id-preview--68123838-66c6-4e6a-82aa-7801aa444a83.lovable.app",
    "http://localhost:5173",
    "http://localhost:3000"
)

           // .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ─── JWT Authentication ───────────────────────────────────────────────────────
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtSecret  = jwtSection["Secret"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer           = true,
            ValidIssuer              = jwtSection["Issuer"],
            ValidateAudience         = true,
            ValidAudience            = jwtSection["Audience"],
            ValidateLifetime         = true,
            ClockSkew                = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

// ─── AutoMapper ───────────────────────────────────────────────────────────────
builder.Services.AddAutoMapper(typeof(MappingProfile));

// ─── FluentValidation ─────────────────────────────────────────────────────────
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

// ─── Repositories + Unit of Work ─────────────────────────────────────────────
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ─── Application Services ─────────────────────────────────────────────────────
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFeedService, FeedService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IPoolService, PoolService>();
builder.Services.AddScoped<IProfileService, ProfileService>();

// ─── Controllers ──────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Let FluentValidation handle validation errors as 400s naturally
        options.SuppressModelStateInvalidFilter = false;
    });

// ─── Swagger / OpenAPI ────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "🎬 CinePool API",
        Version     = "v1",
        Description = "Social movie discovery + OTT subscription pooling API.\n\n" +
                      "**Seed credentials:**\n" +
                      "- alice@cinepool.app / Password123!\n" +
                      "- bob@cinepool.app / Password123!\n" +
                      "- carol@cinepool.app / Password123!\n\n" +
                      "Login via `POST /api/auth/login`, then click **Authorize** and paste the `accessToken`.",
        Contact = new OpenApiContact
        {
            Name  = "CinePool Team",
            Email = "dev@cinepool.app"
        }
    });

    // JWT Bearer in Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter your JWT access token (without 'Bearer ' prefix)."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    options.EnableAnnotations();
});

// ─── Build ────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ─── Auto-migrate + Seed ──────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CinePoolDbContext>();
    try
    {
        await db.Database.MigrateAsync();
        await DatabaseSeeder.SeedAsync(db);
        app.Logger.LogInformation("✅ Database migrated and seeded.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "❌ Database migration/seeding failed: {Message}", ex.Message);
    }
}

// ─── Middleware Pipeline ──────────────────────────────────────────────────────
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CinePool API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "CinePool API";
        options.DefaultModelsExpandDepth(-1);
    });
}

app.UseHttpsRedirection();
app.UseCors("LovableApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status    = "healthy",
    app       = "CinePool API",
    version   = "1.0.0",
    timestamp = DateTime.UtcNow
})).WithTags("Health").WithSummary("API health check");

app.Run();
