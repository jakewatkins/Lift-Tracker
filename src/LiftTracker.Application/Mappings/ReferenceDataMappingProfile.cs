using AutoMapper;
using LiftTracker.Application.DTOs;
using LiftTracker.Domain.Entities;

namespace LiftTracker.Application.Mappings;

/// <summary>
/// AutoMapper profile for reference data entity mappings
/// </summary>
public class ReferenceDataMappingProfile : Profile
{
    public ReferenceDataMappingProfile()
    {
        // ExerciseType entity to DTO mapping
        CreateMap<ExerciseType, ExerciseTypeDto>();

        // MetconType entity to DTO mapping
        CreateMap<MetconType, MetconTypeDto>();

        // MovementType entity to DTO mapping
        CreateMap<MovementType, MovementTypeDto>();
    }
}
