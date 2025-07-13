using HRService.Domain.Common;

namespace HRService.Domain.Entities;

public class Employee : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DateTime HireDate { get; set; }
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
    public Address? Address { get; set; }
    public EmergencyContact? EmergencyContact { get; set; }

    // Navigation properties
    public ICollection<PayrollRecord> PayrollRecords { get; set; } = new List<PayrollRecord>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}

public enum EmployeeStatus
{
    Active,
    Inactive,
    Terminated,
    OnLeave
}

public class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public class EmergencyContact
{
    public string Name { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
