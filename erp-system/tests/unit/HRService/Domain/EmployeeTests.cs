using Xunit;
using FluentAssertions;
using HRService.Domain.Entities;

namespace HRService.UnitTests.Domain;

public class EmployeeTests
{
    [Fact]
    public void Employee_ShouldBeCreated_WithValidData()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@company.com";
        var department = "IT";
        var position = "Software Developer";
        var salary = 75000m;

        // Act
        var employee = new Employee
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Department = department,
            Position = position,
            Salary = salary,
            HireDate = DateTime.UtcNow,
            DateOfBirth = new DateTime(1990, 1, 1),
            EmployeeNumber = "EMP001"
        };

        // Assert
        employee.FirstName.Should().Be(firstName);
        employee.LastName.Should().Be(lastName);
        employee.Email.Should().Be(email);
        employee.Department.Should().Be(department);
        employee.Position.Should().Be(position);
        employee.Salary.Should().Be(salary);
        employee.Status.Should().Be(EmployeeStatus.Active);
        employee.Id.Should().NotBeEmpty();
        employee.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Employee_ShouldHaveDefaultStatus_WhenCreated()
    {
        // Arrange & Act
        var employee = new Employee();

        // Assert
        employee.Status.Should().Be(EmployeeStatus.Active);
        employee.IsDeleted.Should().BeFalse();
        employee.PayrollRecords.Should().NotBeNull().And.BeEmpty();
        employee.LeaveRequests.Should().NotBeNull().And.BeEmpty();
    }

    [Theory]
    [InlineData(EmployeeStatus.Active)]
    [InlineData(EmployeeStatus.Inactive)]
    [InlineData(EmployeeStatus.Terminated)]
    [InlineData(EmployeeStatus.OnLeave)]
    public void Employee_ShouldAllowValidStatusValues(EmployeeStatus status)
    {
        // Arrange
        var employee = new Employee();

        // Act
        employee.Status = status;

        // Assert
        employee.Status.Should().Be(status);
    }
}
