using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StrikeballApi.Application.Common;
using StrikeballApi.Application.Features.Airsofters.Queries;
using StrikeballApi.Domain.Entities;
using Xunit;
using FluentAssertions;

namespace StrikeballApi.Application.Tests.Features.Airsofters;

public class GetAirsofterByIdQueryTests
{
    private readonly IMapper _mapper;
    private readonly TestDbContext _context;

    public GetAirsofterByIdQueryTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: $"ByIdTest_{Guid.NewGuid()}")
            .Options;
        _context = new TestDbContext(options);
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
    }

    [Fact]
    public async Task Handle_ExistingAirsofter_ReturnsDetailsDto()
    {
        var airsofterId = Guid.NewGuid();
        _context.Airsofters.Add(new Airsofter
        {
            Id = airsofterId,
            FirstName = "John",
            LastName = "Doe",
            Callsign = "JDoe"
        });
        await _context.SaveChangesAsync();

        var handler = new GetAirsofterByIdQueryHandler(_context, _mapper);
        var result = await handler.Handle(
            new GetAirsofterByIdQuery { Id = airsofterId }, CancellationToken.None);

        result.Should().NotBeNull();
        result.Callsign.Should().Be("JDoe");
    }

    [Fact]
    public async Task Handle_NonExistingAirsofter_ThrowsNotFoundException()
    {
        var handler = new GetAirsofterByIdQueryHandler(_context, _mapper);

        await FluentActions.Invoking(() => handler.Handle(
            new GetAirsofterByIdQuery { Id = Guid.NewGuid() }, CancellationToken.None))
            .Should().ThrowAsync<Application.Common.Exceptions.NotFoundException>();
    }
}
