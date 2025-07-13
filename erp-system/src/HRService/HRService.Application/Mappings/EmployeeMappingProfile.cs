using AutoMapper;
using HRService.Application.DTOs;
using HRService.Domain.Entities;

namespace HRService.Application.Mappings;

public class EmployeeMappingProfile : Profile
{
    public EmployeeMappingProfile()
    {
        // Employee mappings
        CreateMap<Employee, EmployeeDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<CreateEmployeeDto, Employee>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeNumber, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => EmployeeStatus.Active))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.PayrollRecords, opt => opt.Ignore())
            .ForMember(dest => dest.LeaveRequests, opt => opt.Ignore());

        CreateMap<UpdateEmployeeDto, Employee>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeNumber, opt => opt.Ignore())
            .ForMember(dest => dest.DateOfBirth, opt => opt.Ignore())
            .ForMember(dest => dest.HireDate, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.PayrollRecords, opt => opt.Ignore())
            .ForMember(dest => dest.LeaveRequests, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => 
                Enum.IsDefined(typeof(EmployeeStatus), src.Status) 
                    ? Enum.Parse<EmployeeStatus>(src.Status) 
                    : EmployeeStatus.Active));

        // Address mappings
        CreateMap<Address, AddressDto>().ReverseMap();

        // Emergency contact mappings
        CreateMap<EmergencyContact, EmergencyContactDto>().ReverseMap();
    }
}
