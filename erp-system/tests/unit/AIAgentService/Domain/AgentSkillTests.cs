using Xunit;
using FluentAssertions;
using AIAgentService.Domain.Entities;

namespace AIAgentService.UnitTests.Domain
{
    public class AgentSkillTests
    {
        [Fact]
        public void AgentSkill_WhenCreated_ShouldHaveCorrectInitialState()
        {
            // Arrange
            var name = "TestSkill";
            var description = "Test skill description";
            var serviceName = "TestService";
            var skillType = "Query";
            var configuration = "{\"endpoint\": \"/api/test\"}";
            var version = "1.0";

            // Act
            var skill = new AgentSkill(name, description, serviceName, skillType, configuration, version);

            // Assert
            skill.Name.Should().Be(name);
            skill.Description.Should().Be(description);
            skill.ServiceName.Should().Be(serviceName);
            skill.SkillType.Should().Be(skillType);
            skill.Configuration.Should().Be(configuration);
            skill.Version.Should().Be(version);
            skill.IsActive.Should().BeTrue();
            skill.Id.Should().NotBeEmpty();
            skill.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void AgentSkill_WhenNameIsNullOrEmpty_ShouldThrowArgumentNullException(string? name)
        {
            // Act & Assert
            var act = () => new AgentSkill(name!, "description", "service", "type", "config");
            act.Should().Throw<ArgumentNullException>().WithParameterName("name");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void AgentSkill_WhenDescriptionIsNullOrEmpty_ShouldThrowArgumentNullException(string? description)
        {
            // Act & Assert
            var act = () => new AgentSkill("name", description!, "service", "type", "config");
            act.Should().Throw<ArgumentNullException>().WithParameterName("description");
        }

        [Fact]
        public void Activate_WhenSkillIsInactive_ShouldActivateSkill()
        {
            // Arrange
            var skill = new AgentSkill("TestSkill", "Description", "Service", "Type", "Config");
            skill.Deactivate();

            // Act
            skill.Activate();

            // Assert
            skill.IsActive.Should().BeTrue();
            skill.UpdatedAt.Should().NotBeNull();
        }

        [Fact]
        public void Deactivate_WhenSkillIsActive_ShouldDeactivateSkill()
        {
            // Arrange
            var skill = new AgentSkill("TestSkill", "Description", "Service", "Type", "Config");

            // Act
            skill.Deactivate();

            // Assert
            skill.IsActive.Should().BeFalse();
            skill.UpdatedAt.Should().NotBeNull();
        }

        [Fact]
        public void UpdateConfiguration_ShouldUpdateConfigurationAndTimestamp()
        {
            // Arrange
            var skill = new AgentSkill("TestSkill", "Description", "Service", "Type", "Config");
            var newConfiguration = "{\"new\": \"config\"}";
            var originalUpdatedAt = skill.UpdatedAt;

            // Act
            skill.UpdateConfiguration(newConfiguration);

            // Assert
            skill.Configuration.Should().Be(newConfiguration);
            skill.UpdatedAt.Should().BeAfter(originalUpdatedAt ?? DateTime.MinValue);
        }

        [Fact]
        public void UpdateConfiguration_WhenConfigurationIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var skill = new AgentSkill("TestSkill", "Description", "Service", "Type", "Config");

            // Act & Assert
            var act = () => skill.UpdateConfiguration(null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
        }

        [Fact]
        public void UpdateVersion_ShouldUpdateVersionAndTimestamp()
        {
            // Arrange
            var skill = new AgentSkill("TestSkill", "Description", "Service", "Type", "Config", "1.0");
            var newVersion = "2.0";
            var originalUpdatedAt = skill.UpdatedAt;

            // Act
            skill.UpdateVersion(newVersion);

            // Assert
            skill.Version.Should().Be(newVersion);
            skill.UpdatedAt.Should().BeAfter(originalUpdatedAt ?? DateTime.MinValue);
        }
    }
}
