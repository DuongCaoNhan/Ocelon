using Xunit;
using FluentAssertions;
using AIAgentService.Domain.Entities;

namespace AIAgentService.UnitTests.Domain
{
    public class AgentSessionTests
    {
        [Fact]
        public void AgentSession_WhenCreated_ShouldHaveCorrectInitialState()
        {
            // Arrange
            var userId = "test-user-123";
            var sessionName = "Test Session";
            var description = "Test Description";
            var context = "Test Context";

            // Act
            var session = new AgentSession(userId, sessionName, description, context);

            // Assert
            session.UserId.Should().Be(userId);
            session.SessionName.Should().Be(sessionName);
            session.Description.Should().Be(description);
            session.Context.Should().Be(context);
            session.Status.Should().Be(AgentSessionStatus.Active);
            session.Id.Should().NotBeEmpty();
            session.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            session.EndedAt.Should().BeNull();
            session.Messages.Should().BeEmpty();
        }

        [Fact]
        public void AgentSession_WhenUserIdIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            string? userId = null;
            var sessionName = "Test Session";

            // Act & Assert
            var act = () => new AgentSession(userId!, sessionName);
            act.Should().Throw<ArgumentNullException>().WithParameterName("userId");
        }

        [Fact]
        public void AgentSession_WhenSessionNameIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var userId = "test-user-123";
            string? sessionName = null;

            // Act & Assert
            var act = () => new AgentSession(userId, sessionName!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("sessionName");
        }

        [Fact]
        public void AddMessage_WhenSessionIsActive_ShouldAddMessage()
        {
            // Arrange
            var session = new AgentSession("test-user", "Test Session");
            var content = "Hello, this is a test message";

            // Act
            session.AddMessage(content, MessageType.User);

            // Assert
            session.Messages.Should().HaveCount(1);
            var message = session.Messages.First();
            message.Content.Should().Be(content);
            message.MessageType.Should().Be(MessageType.User);
            message.SessionId.Should().Be(session.Id);
        }

        [Fact]
        public void AddMessage_WhenSessionIsEnded_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var session = new AgentSession("test-user", "Test Session");
            session.EndSession();

            // Act & Assert
            var act = () => session.AddMessage("Test message", MessageType.User);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Cannot add messages to inactive session");
        }

        [Fact]
        public void EndSession_WhenSessionIsActive_ShouldEndSession()
        {
            // Arrange
            var session = new AgentSession("test-user", "Test Session");

            // Act
            session.EndSession();

            // Assert
            session.Status.Should().Be(AgentSessionStatus.Ended);
            session.EndedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void EndSession_WhenSessionIsAlreadyEnded_ShouldNotThrow()
        {
            // Arrange
            var session = new AgentSession("test-user", "Test Session");
            session.EndSession();
            var firstEndTime = session.EndedAt;

            // Act
            session.EndSession();

            // Assert
            session.Status.Should().Be(AgentSessionStatus.Ended);
            session.EndedAt.Should().Be(firstEndTime);
        }

        [Fact]
        public void UpdateContext_ShouldUpdateContextAndTimestamp()
        {
            // Arrange
            var session = new AgentSession("test-user", "Test Session");
            var newContext = "Updated context";
            var originalUpdatedAt = session.UpdatedAt;

            // Act
            session.UpdateContext(newContext);

            // Assert
            session.Context.Should().Be(newContext);
            session.UpdatedAt.Should().BeAfter(originalUpdatedAt ?? DateTime.MinValue);
        }
    }
}
