using FluentValidation;
using AIAgentService.Application.Commands;

namespace AIAgentService.Application.Validators
{
    public class CreateAgentSessionCommandValidator : AbstractValidator<CreateAgentSessionCommand>
    {
        public CreateAgentSessionCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required")
                .MaximumLength(100)
                .WithMessage("User ID cannot exceed 100 characters");

            RuleFor(x => x.SessionName)
                .NotEmpty()
                .WithMessage("Session name is required")
                .MaximumLength(200)
                .WithMessage("Session name cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Description cannot exceed 1000 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.Context)
                .MaximumLength(5000)
                .WithMessage("Context cannot exceed 5000 characters")
                .When(x => !string.IsNullOrEmpty(x.Context));
        }
    }

    public class SendMessageToAgentCommandValidator : AbstractValidator<SendMessageToAgentCommand>
    {
        public SendMessageToAgentCommandValidator()
        {
            RuleFor(x => x.SessionId)
                .NotEmpty()
                .WithMessage("Session ID is required");

            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("Message content is required")
                .MaximumLength(10000)
                .WithMessage("Message content cannot exceed 10000 characters");

            RuleFor(x => x.Metadata)
                .MaximumLength(2000)
                .WithMessage("Metadata cannot exceed 2000 characters")
                .When(x => !string.IsNullOrEmpty(x.Metadata));
        }
    }

    public class CreateAgentSkillCommandValidator : AbstractValidator<CreateAgentSkillCommand>
    {
        public CreateAgentSkillCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Skill name is required")
                .MaximumLength(100)
                .WithMessage("Skill name cannot exceed 100 characters");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Skill description is required")
                .MaximumLength(500)
                .WithMessage("Skill description cannot exceed 500 characters");

            RuleFor(x => x.ServiceName)
                .NotEmpty()
                .WithMessage("Service name is required")
                .MaximumLength(100)
                .WithMessage("Service name cannot exceed 100 characters");

            RuleFor(x => x.SkillType)
                .NotEmpty()
                .WithMessage("Skill type is required")
                .MaximumLength(50)
                .WithMessage("Skill type cannot exceed 50 characters");

            RuleFor(x => x.Configuration)
                .NotEmpty()
                .WithMessage("Configuration is required");

            RuleFor(x => x.Version)
                .MaximumLength(20)
                .WithMessage("Version cannot exceed 20 characters")
                .When(x => !string.IsNullOrEmpty(x.Version));
        }
    }
}
