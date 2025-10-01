using AutoMapper;
using LiftTracker.Application.DTOs;
using LiftTracker.Domain.Entities;

namespace LiftTracker.Application.Mappings;

/// <summary>
/// AutoMapper profile for StrengthLift entity mappings
/// </summary>
public class StrengthLiftMappingProfile : Profile
{
    public StrengthLiftMappingProfile()
    {
        // StrengthLift entity to DTO mappings
        CreateMap<StrengthLift, StrengthLiftDto>()
            .ForMember(dest => dest.ExerciseTypeName, opt => opt.MapFrom(src => src.ExerciseType!.Name));

        // Create strength lift DTO to entity mapping
        CreateMap<CreateStrengthLiftDto, StrengthLift>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.WorkoutSession, opt => opt.Ignore())
            .ForMember(dest => dest.ExerciseType, opt => opt.Ignore());

        // Update strength lift DTO to entity mapping
        CreateMap<UpdateStrengthLiftDto, StrengthLift>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.WorkoutSessionId, opt => opt.Ignore())
            .ForMember(dest => dest.WorkoutSession, opt => opt.Ignore())
            .ForMember(dest => dest.ExerciseType, opt => opt.Ignore());
    }
}
