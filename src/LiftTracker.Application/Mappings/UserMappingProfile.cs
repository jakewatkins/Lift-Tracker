using AutoMapper;
using LiftTracker.Application.DTOs;
using LiftTracker.Domain.Entities;

namespace LiftTracker.Application.Mappings;

/// <summary>
/// AutoMapper profile for User entity mappings
/// </summary>
public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        // User entity to DTO mappings
        CreateMap<User, UserDto>();

        // Create user DTO to entity mapping
        CreateMap<CreateUserDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.LastLoginDate, opt => opt.Ignore())
            .ForMember(dest => dest.WorkoutSessions, opt => opt.Ignore());

        // Update user DTO to entity mapping (for updates)
        CreateMap<UpdateUserDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastLoginDate, opt => opt.Ignore())
            .ForMember(dest => dest.WorkoutSessions, opt => opt.Ignore());
    }
}
