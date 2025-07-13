using MediatR;
using HRService.Application.DTOs;

namespace HRService.Application.Queries;

public record GetEmployeeByIdQuery(Guid Id) : IRequest<EmployeeDto?>;

public record GetEmployeeByEmailQuery(string Email) : IRequest<EmployeeDto?>;

public record GetEmployeeByEmployeeNumberQuery(string EmployeeNumber) : IRequest<EmployeeDto?>;

public record GetAllEmployeesQuery : IRequest<IEnumerable<EmployeeDto>>;

public record GetEmployeesByDepartmentQuery(string Department) : IRequest<IEnumerable<EmployeeDto>>;
