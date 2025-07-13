using MediatR;
using HRService.Application.DTOs;

namespace HRService.Application.Commands;

public record CreateEmployeeCommand(CreateEmployeeDto Employee) : IRequest<EmployeeDto>;

public record UpdateEmployeeCommand(Guid Id, UpdateEmployeeDto Employee) : IRequest<EmployeeDto>;

public record DeleteEmployeeCommand(Guid Id) : IRequest<bool>;
