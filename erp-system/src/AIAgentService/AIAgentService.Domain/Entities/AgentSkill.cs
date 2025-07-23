using AIAgentService.Domain.Common;

namespace AIAgentService.Domain.Entities
{
    /// <summary>
    /// Represents an AI skill that can be executed by the agent
    /// </summary>
    public class AgentSkill : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public string ServiceName { get; private set; } = string.Empty;
        public string SkillType { get; private set; } = string.Empty;
        public string Configuration { get; private set; } = string.Empty;
        public bool IsActive { get; private set; }
        public string? Version { get; private set; }

        private AgentSkill() { } // For EF Core

        public AgentSkill(string name, string description, string serviceName, string skillType, string configuration, string? version = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            ServiceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
            SkillType = skillType ?? throw new ArgumentNullException(nameof(skillType));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Version = version;
            IsActive = true;
        }

        public void Activate()
        {
            IsActive = true;
            SetUpdatedInfo();
        }

        public void Deactivate()
        {
            IsActive = false;
            SetUpdatedInfo();
        }

        public void UpdateConfiguration(string configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            SetUpdatedInfo();
        }

        public void UpdateVersion(string version)
        {
            Version = version;
            SetUpdatedInfo();
        }
    }
}
