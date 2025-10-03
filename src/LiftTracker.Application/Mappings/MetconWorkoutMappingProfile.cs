using AutoMapper;
using LiftTracker.Application.DTOs;
using LiftTracker.Domain.Entities;

namespace LiftTracker.Application.Mappings;

/// <summary>
/// AutoMapper profile for MetconWorkout entity mappings
/// </summary>
public class MetconWorkoutMappingProfile : Profile
{
    public MetconWorkoutMappingProfile()
    {
        // MetconWorkout entity to DTO mappings
        CreateMap<MetconWorkout, MetconWorkoutDto>()
            .ForMember(dest => dest.MetconTypeName, opt => opt.MapFrom(src => src.MetconType!.Name))
            .ForMember(dest => dest.Movements, opt => opt.MapFrom(src => src.MetconMovements));

        // Create metcon workout DTO to entity mapping
        CreateMap<CreateMetconWorkoutDto, MetconWorkout>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.WorkoutSession, opt => opt.Ignore())
            .ForMember(dest => dest.MetconType, opt => opt.Ignore())
            .ForMember(dest => dest.MetconMovements, opt => opt.Ignore());

        // Update metcon workout DTO to entity mapping
        CreateMap<UpdateMetconWorkoutDto, MetconWorkout>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.WorkoutSessionId, opt => opt.Ignore())
            .ForMember(dest => dest.WorkoutSession, opt => opt.Ignore())
            .ForMember(dest => dest.MetconType, opt => opt.Ignore())
            .ForMember(dest => dest.MetconMovements, opt => opt.Ignore());
    }
}
