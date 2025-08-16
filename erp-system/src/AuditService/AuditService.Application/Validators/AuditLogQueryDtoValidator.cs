using AuditService.Application.DTOs;
using FluentValidation;

namespace AuditService.Application.Validators;

/// <summary>
/// Validator for audit log query parameters
/// Ensures valid input and reasonable limits
/// </summary>
public class AuditLogQueryDtoValidator : AbstractValidator<AuditLogQueryDto>
{
    public AuditLogQueryDtoValidator()
    {
        // Pagination validation
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(1000)
            .WithMessage("Page size must be between 1 and 1000");

        // Date range validation
        RuleFor(x => x)
            .Must(x => x.FromDate == null || x.ToDate == null || x.FromDate <= x.ToDate)
            .WithMessage("From date must be less than or equal to to date");

        // Date range shouldn't be too large for performance
        RuleFor(x => x)
            .Must(x => x.FromDate == null || x.ToDate == null || 
                      x.ToDate.Value.Subtract(x.FromDate.Value).TotalDays <= 365)
            .WithMessage("Date range cannot exceed 365 days")
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);

        // Sort validation
        RuleFor(x => x.SortBy)
            .Must(BeValidSortField)
            .WithMessage("Invalid sort field. Valid fields: Timestamp, ServiceName, UserId, ActionType, EntityType, Severity");

        RuleFor(x => x.SortDirection)
            .Must(x => x.Equals("asc", StringComparison.OrdinalIgnoreCase) || 
                      x.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Sort direction must be 'asc' or 'desc'");

        // String length validations
        RuleFor(x => x.UserId)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.UserId));

        RuleFor(x => x.ServiceName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.ServiceName));

        RuleFor(x => x.EntityType)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.EntityType));

        RuleFor(x => x.EntityId)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.EntityId));

        RuleFor(x => x.CorrelationId)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.CorrelationId));

        RuleFor(x => x.IpAddress)
            .MaximumLength(45)
            .When(x => !string.IsNullOrEmpty(x.IpAddress));

        RuleFor(x => x.Source)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Source));

        RuleFor(x => x.Tags)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Tags));

        RuleFor(x => x.RetentionCategory)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.RetentionCategory));
    }

    private static bool BeValidSortField(string sortField)
    {
        var validFields = new[]
        {
            "Timestamp", "ServiceName", "UserId", "ActionType", 
            "EntityType", "Severity", "Result", "CorrelationId"
        };

        return validFields.Contains(sortField, StringComparer.OrdinalIgnoreCase);
    }
}
