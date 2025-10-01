using AutoMapper;
using LiftTracker.Application.DTOs;
using LiftTracker.Domain.Entities;

namespace LiftTracker.Application.Mappings;

/// <summary>
/// AutoMapper profile for MetconMovement entity mappings
/// </summary>
public class MetconMovementMappingProfile : Profile
{
    public MetconMovementMappingProfile()
    {
        // MetconMovement entity to DTO mappings
        CreateMap<MetconMovement, MetconMovementDto>()
            .ForMember(dest => dest.MovementTypeName, opt => opt.MapFrom(src => src.MovementType!.Name));

        // Create metcon movement DTO to entity mapping
        CreateMap<CreateMetconMovementDto, MetconMovement>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.MetconWorkout, opt => opt.Ignore())
            .ForMember(dest => dest.MovementType, opt => opt.Ignore());

        // Update metcon movement DTO to entity mapping
        CreateMap<UpdateMetconMovementDto, MetconMovement>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.MetconWorkoutId, opt => opt.Ignore())
            .ForMember(dest => dest.MetconWorkout, opt => opt.Ignore())
            .ForMember(dest => dest.MovementType, opt => opt.Ignore());
    }
}
