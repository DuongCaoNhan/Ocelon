using MediatR;
using AutoMapper;
using HRService.Application.Commands;
using HRService.Application.DTOs;
using HRService.Domain.Entities;
using HRService.Domain.Repositories;

namespace HRService.Application.Handlers;

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, EmployeeDto>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IMapper _mapper;

    public CreateEmployeeCommandHandler(IEmployeeRepository employeeRepository, IMapper mapper)
    {
        _employeeRepository = employeeRepository;
        _mapper = mapper;
    }

    public async Task<EmployeeDto> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        // Check if employee with email already exists
        var existingEmployee = await _employeeRepository.GetByEmailAsync(request.Employee.Email, cancellationToken);
        if (existingEmployee != null)
        {
            throw new InvalidOperationException($"Employee with email {request.Employee.Email} already exists.");
        }

        // Map DTO to entity
        var employee = _mapper.Map<Employee>(request.Employee);
        
        // Generate employee number (in real implementation, this might be more sophisticated)
        employee.EmployeeNumber = $"EMP{DateTime.UtcNow:yyyyMMdd}{Random.Shared.Next(1000, 9999)}";

        // Save employee
        var createdEmployee = await _employeeRepository.AddAsync(employee, cancellationToken);

        // Map back to DTO
        return _mapper.Map<EmployeeDto>(createdEmployee);
    }
}

public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, EmployeeDto>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IMapper _mapper;

    public UpdateEmployeeCommandHandler(IEmployeeRepository employeeRepository, IMapper mapper)
    {
        _employeeRepository = employeeRepository;
        _mapper = mapper;
    }

    public async Task<EmployeeDto> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (employee == null)
        {
            throw new ArgumentException($"Employee with ID {request.Id} not found.");
        }

        // Update employee properties
        _mapper.Map(request.Employee, employee);
        employee.UpdatedAt = DateTime.UtcNow;

        await _employeeRepository.UpdateAsync(employee, cancellationToken);

        return _mapper.Map<EmployeeDto>(employee);
    }
}

public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand, bool>
{
    private readonly IEmployeeRepository _employeeRepository;

    public DeleteEmployeeCommandHandler(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<bool> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        var exists = await _employeeRepository.ExistsAsync(request.Id, cancellationToken);
        if (!exists)
        {
            return false;
        }

        await _employeeRepository.DeleteAsync(request.Id, cancellationToken);
        return true;
    }
}
