using HRService.Domain.Common;

namespace HRService.Domain.Entities;

public class PayrollRecord : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public DateTime PayPeriodStart { get; set; }
    public DateTime PayPeriodEnd { get; set; }
    public decimal GrossPay { get; set; }
    public decimal NetPay { get; set; }
    public decimal TaxWithheld { get; set; }
    public decimal OtherDeductions { get; set; }
    public int HoursWorked { get; set; }
    public int OvertimeHours { get; set; }
    public PayrollStatus Status { get; set; } = PayrollStatus.Pending;
}

public enum PayrollStatus
{
    Pending,
    Processed,
    Paid,
    Cancelled
}
