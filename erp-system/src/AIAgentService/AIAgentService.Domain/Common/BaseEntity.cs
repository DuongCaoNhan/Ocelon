namespace AIAgentService.Domain.Common
{
    /// <summary>
    /// Base entity class providing common properties and behavior for all domain entities
    /// </summary>
    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; protected set; }
        public string? CreatedBy { get; protected set; }
        public string? UpdatedBy { get; protected set; }

        protected void SetUpdatedInfo(string? updatedBy = null)
        {
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }
    }
}
