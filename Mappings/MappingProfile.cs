using AutoMapper;
using CinePool.API.DTOs;
using CinePool.API.Models;

namespace CinePool.API.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User → UserDto
        CreateMap<User, UserDto>();

        // User → ProfileDto (stats populated in service layer)
        CreateMap<User, ProfileDto>()
            .ForMember(dest => dest.ReviewCount,  opt => opt.Ignore())
            .ForMember(dest => dest.WatchedCount, opt => opt.Ignore())
            .ForMember(dest => dest.BoostCount,   opt => opt.Ignore())
            .ForMember(dest => dest.PoolCount,    opt => opt.Ignore());

        // Movie → MovieDto
        CreateMap<Movie, MovieDto>()
            .ForMember(dest => dest.Platforms,
                opt => opt.MapFrom(src => src.Platforms.Select(p => p.Platform.ToString()).ToList()));

        // Movie → MovieDetailDto
        CreateMap<Movie, MovieDetailDto>()
            .ForMember(dest => dest.Platforms,
                opt => opt.MapFrom(src => src.Platforms.Select(p => p.Platform.ToString()).ToList()))
            .ForMember(dest => dest.RecentReviews,   opt => opt.Ignore())
            .ForMember(dest => dest.TotalReviews,    opt => opt.Ignore())
            .ForMember(dest => dest.AverageUserRating, opt => opt.Ignore());

        // Review → ReviewDto
        CreateMap<Review, ReviewDto>()
            .ForMember(dest => dest.BoostCount,    opt => opt.MapFrom(src => src.Boosts.Count))
            .ForMember(dest => dest.RecentBoosts,  opt => opt.MapFrom(src => src.Boosts.OrderByDescending(b => b.CreatedAt).Take(3).ToList()));

        // Boost → BoostDto
        CreateMap<Boost, BoostDto>();

        // Pool → PoolDto
        CreateMap<Pool, PoolDto>()
            .ForMember(dest => dest.Platform,       opt => opt.MapFrom(src => src.Platform.ToString()))
            .ForMember(dest => dest.AvailableSlots, opt => opt.MapFrom(src => src.TotalSlots - src.FilledSlots))
            .ForMember(dest => dest.IsExpired,      opt => opt.MapFrom(src => src.ExpiresAt < DateTime.UtcNow))
            .ForMember(dest => dest.IsFull,         opt => opt.MapFrom(src => src.FilledSlots >= src.TotalSlots));

        // WatchedMovie → WatchedMovieDto
        CreateMap<WatchedMovie, WatchedMovieDto>();

        // UserPreferences → UserPreferencesDto
        CreateMap<UserPreferences, UserPreferencesDto>()
            .ForMember(dest => dest.FavoriteGenres,   opt => opt.MapFrom(src => src.FavoriteGenresList))
            .ForMember(dest => dest.FavoritePlatforms, opt => opt.MapFrom(src => src.FavoritePlatformsList));
    }
}
