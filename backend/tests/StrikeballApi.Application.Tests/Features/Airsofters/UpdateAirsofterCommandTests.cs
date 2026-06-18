using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StrikeballApi.Application.Common.Interfaces;
using StrikeballApi.Application.Features.Airsofters.Commands;
using StrikeballApi.Domain.Entities;
using Xunit;
using FluentAssertions;

namespace StrikeballApi.Application.Tests.Features.Airsofters;

public class UpdateAirsofterCommandTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly TestDbContext _context;

    public UpdateAirsofterCommandTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: $"UpdateTest_{Guid.NewGuid()}")
            .Options;
        _context = new TestDbContext(options);
        _currentUserMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Handle_OwnProfile_UpdatesSuccessfully()
    {
        var userId = Guid.NewGuid();
        var airsofterId = Guid.NewGuid();
        _context.Airsofters.Add(new Airsofter
        {
            Id = airsofterId,
            FirstName = "OldName",
            LastName = "OldLast",
            Callsign = "OldCallsign",
            UserId = userId
        });
        await _context.SaveChangesAsync();

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _currentUserMock.Setup(x => x.IsAdministrator).Returns(false);

        var handler = new UpdateAirsofterCommandHandler(
            _context, _currentUserMock.Object,
            NullLogger<UpdateAirsofterCommandHandler>.Instance);

        var command = new UpdateAirsofterCommand
        {
            Id = airsofterId,
            FirstName = "NewName",
            LastName = "NewLast",
            KitDescription = "Updated kit"
        };

        await handler.Handle(command, CancellationToken.None);

        var updated = await _context.Airsofters.FindAsync(airsofterId);
        updated.Should().NotBeNull();
        updated!.FirstName.Should().Be("NewName");
        updated.LastName.Should().Be("NewLast");
        updated.KitDescription.Should().Be("Updated kit");
    }

    [Fact]
    public async Task Handle_OtherUserProfile_ThrowsUnauthorizedException()
    {
        var airsofterId = Guid.NewGuid();
        _context.Airsofters.Add(new Airsofter
        {
            Id = airsofterId,
            FirstName = "Test",
            LastName = "User",
            Callsign = "TestUser",
            UserId = Guid.NewGuid()
        });
        await _context.SaveChangesAsync();

        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
        _currentUserMock.Setup(x => x.IsAdministrator).Returns(false);

        var handler = new UpdateAirsofterCommandHandler(
            _context, _currentUserMock.Object,
            NullLogger<UpdateAirsofterCommandHandler>.Instance);

        var command = new UpdateAirsofterCommand
        {
            Id = airsofterId,
            FirstName = "Hacker",
            LastName = "Bad"
        };

        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Application.Common.Exceptions.UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_NonExistingAirsofter_ThrowsNotFoundException()
    {
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid());

        var handler = new UpdateAirsofterCommandHandler(
            _context, _currentUserMock.Object,
            NullLogger<UpdateAirsofterCommandHandler>.Instance);

        var command = new UpdateAirsofterCommand
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User"
        };

        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Application.Common.Exceptions.NotFoundException>();
    }
}
