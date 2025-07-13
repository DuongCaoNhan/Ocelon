using Microsoft.AspNetCore.Mvc;
using MediatR;
using HRService.Application.Commands;
using HRService.Application.Queries;
using HRService.Application.DTOs;

namespace HRService.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class EmployeesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(IMediator mediator, ILogger<EmployeesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all employees
    /// </summary>
    /// <returns>List of employees</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EmployeeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAllEmployees()
    {
        _logger.LogInformation("Getting all employees");
        var employees = await _mediator.Send(new GetAllEmployeesQuery());
        return Ok(employees);
    }

    /// <summary>
    /// Get employee by ID
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <returns>Employee details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeDto>> GetEmployeeById(Guid id)
    {
        _logger.LogInformation("Getting employee with ID: {EmployeeId}", id);
        var employee = await _mediator.Send(new GetEmployeeByIdQuery(id));
        
        if (employee == null)
        {
            _logger.LogWarning("Employee with ID {EmployeeId} not found", id);
            return NotFound();
        }

        return Ok(employee);
    }

    /// <summary>
    /// Get employee by email
    /// </summary>
    /// <param name="email">Employee email</param>
    /// <returns>Employee details</returns>
    [HttpGet("by-email/{email}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeDto>> GetEmployeeByEmail(string email)
    {
        _logger.LogInformation("Getting employee with email: {Email}", email);
        var employee = await _mediator.Send(new GetEmployeeByEmailQuery(email));
        
        if (employee == null)
        {
            _logger.LogWarning("Employee with email {Email} not found", email);
            return NotFound();
        }

        return Ok(employee);
    }

    /// <summary>
    /// Get employees by department
    /// </summary>
    /// <param name="department">Department name</param>
    /// <returns>List of employees in the department</returns>
    [HttpGet("by-department/{department}")]
    [ProducesResponseType(typeof(IEnumerable<EmployeeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployeesByDepartment(string department)
    {
        _logger.LogInformation("Getting employees in department: {Department}", department);
        var employees = await _mediator.Send(new GetEmployeesByDepartmentQuery(department));
        return Ok(employees);
    }

    /// <summary>
    /// Create a new employee
    /// </summary>
    /// <param name="createEmployeeDto">Employee data</param>
    /// <returns>Created employee</returns>
    [HttpPost]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmployeeDto>> CreateEmployee([FromBody] CreateEmployeeDto createEmployeeDto)
    {
        _logger.LogInformation("Creating new employee: {FirstName} {LastName}", 
            createEmployeeDto.FirstName, createEmployeeDto.LastName);
        
        var employee = await _mediator.Send(new CreateEmployeeCommand(createEmployeeDto));
        
        return CreatedAtAction(
            nameof(GetEmployeeById), 
            new { id = employee.Id }, 
            employee);
    }

    /// <summary>
    /// Update an existing employee
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <param name="updateEmployeeDto">Updated employee data</param>
    /// <returns>Updated employee</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmployeeDto>> UpdateEmployee(Guid id, [FromBody] UpdateEmployeeDto updateEmployeeDto)
    {
        _logger.LogInformation("Updating employee with ID: {EmployeeId}", id);
        
        try
        {
            var employee = await _mediator.Send(new UpdateEmployeeCommand(id, updateEmployeeDto));
            return Ok(employee);
        }
        catch (ArgumentException)
        {
            _logger.LogWarning("Employee with ID {EmployeeId} not found for update", id);
            return NotFound();
        }
    }

    /// <summary>
    /// Delete an employee
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEmployee(Guid id)
    {
        _logger.LogInformation("Deleting employee with ID: {EmployeeId}", id);
        
        var result = await _mediator.Send(new DeleteEmployeeCommand(id));
        
        if (!result)
        {
            _logger.LogWarning("Employee with ID {EmployeeId} not found for deletion", id);
            return NotFound();
        }

        return NoContent();
    }
}
