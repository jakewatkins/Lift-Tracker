using AutoMapper;
using LiftTracker.Application.DTOs;
using LiftTracker.Domain.Entities;

namespace LiftTracker.Application.Mappings;

/// <summary>
/// AutoMapper profile for WorkoutSession entity mappings
/// </summary>
public class WorkoutSessionMappingProfile : Profile
{
    public WorkoutSessionMappingProfile()
    {
        // WorkoutSession entity to DTO mappings
        CreateMap<WorkoutSession, WorkoutSessionDto>()
            .ForMember(dest => dest.StrengthLifts, opt => opt.MapFrom(src => src.StrengthLifts))
            .ForMember(dest => dest.MetconWorkouts, opt => opt.MapFrom(src => src.MetconWorkouts));

        CreateMap<WorkoutSession, WorkoutSessionSummaryDto>()
            .ForMember(dest => dest.StrengthLiftCount, opt => opt.MapFrom(src => src.StrengthLifts.Count))
            .ForMember(dest => dest.MetconWorkoutCount, opt => opt.MapFrom(src => src.MetconWorkouts.Count))
            .ForMember(dest => dest.TotalVolumeLifted, opt => opt.MapFrom(src =>
                src.StrengthLifts
                    .Where(sl => sl.SetStructure == "SetsReps" && sl.Sets.HasValue && sl.Reps.HasValue)
                    .Sum(sl => sl.Weight * sl.Reps!.Value * sl.Sets!.Value)));

        // Create workout session DTO to entity mapping
        CreateMap<CreateWorkoutSessionDto, WorkoutSession>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.StrengthLifts, opt => opt.Ignore())
            .ForMember(dest => dest.MetconWorkouts, opt => opt.Ignore());

        // Update workout session DTO to entity mapping
        CreateMap<UpdateWorkoutSessionDto, WorkoutSession>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.StrengthLifts, opt => opt.Ignore())
            .ForMember(dest => dest.MetconWorkouts, opt => opt.Ignore());
    }
}
