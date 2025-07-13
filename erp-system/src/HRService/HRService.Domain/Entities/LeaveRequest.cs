using HRService.Domain.Common;

namespace HRService.Domain.Entities;

public class LeaveRequest : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public LeaveType Type { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DaysRequested { get; set; }
    public string Reason { get; set; } = string.Empty;
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public string? ApproverComments { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
}

public enum LeaveType
{
    Vacation,
    Sick,
    Personal,
    Maternity,
    Paternity,
    Bereavement,
    Emergency
}

public enum LeaveStatus
{
    Pending,
    Approved,
    Rejected,
    Cancelled
}
