using AuditService.Application.DTOs;
using AuditService.Domain.Entities;
using AuditService.Domain.Events;
using AutoMapper;

namespace AuditService.Application.Mappings;

/// <summary>
/// AutoMapper profile for audit log mappings
/// Handles conversion between domain entities, events, and DTOs
/// </summary>
public class AuditMappingProfile : Profile
{
    public AuditMappingProfile()
    {
        // AuditEvent to AuditLog mapping
        CreateMap<AuditEvent, AuditLog>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Generate new ID
            .ForMember(dest => dest.DataHash, opt => opt.Ignore()) // Calculated separately
            .ForMember(dest => dest.ExpiryDate, opt => opt.Ignore()) // Calculated separately
            .ForMember(dest => dest.SchemaVersion, opt => opt.MapFrom(src => "1.0"));

        // AuditLog to AuditLogDto mapping
        CreateMap<AuditLog, AuditLogDto>()
            .ForMember(dest => dest.ContainsSensitiveData, opt => opt.MapFrom(src => src.ContainsSensitiveData));

        // Reverse mapping for updates
        CreateMap<AuditLogDto, AuditLog>()
            .ForMember(dest => dest.DataHash, opt => opt.Ignore())
            .ForMember(dest => dest.ExpiryDate, opt => opt.Ignore())
            .ForMember(dest => dest.SchemaVersion, opt => opt.Ignore())
            .ForMember(dest => dest.StackTrace, opt => opt.Ignore())
            .ForMember(dest => dest.OriginalData, opt => opt.Ignore())
            .ForMember(dest => dest.NewData, opt => opt.Ignore())
            .ForMember(dest => dest.Metadata, opt => opt.Ignore())
            .ForMember(dest => dest.UserAgent, opt => opt.Ignore())
            .ForMember(dest => dest.TenantId, opt => opt.Ignore());
    }
}
