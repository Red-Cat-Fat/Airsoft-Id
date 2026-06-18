using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StrikeballApi.Application.Common;
using StrikeballApi.Application.Features.Airsofters.Queries;
using StrikeballApi.Domain.Entities;
using Xunit;
using FluentAssertions;

namespace StrikeballApi.Application.Tests.Features.Airsofters;

public class GetAirsoftersListQueryTests
{
    private readonly IMapper _mapper;
    private readonly TestDbContext _context;

    public GetAirsoftersListQueryTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: $"ListTest_{Guid.NewGuid()}")
            .Options;
        _context = new TestDbContext(options);
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
    }

    [Fact]
    public async Task Handle_ReturnsAllAirsofters()
    {
        _context.Airsofters.AddRange(
            new Airsofter { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Callsign = "JDoe" },
            new Airsofter { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith", Callsign = "JSmith" }
        );
        await _context.SaveChangesAsync();

        var handler = new GetAirsoftersListQueryHandler(_context, _mapper);
        var result = await handler.Handle(new GetAirsoftersListQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithCallsignFilter_ReturnsFilteredResults()
    {
        _context.Airsofters.AddRange(
            new Airsofter { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Callsign = "JDoe" },
            new Airsofter { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith", Callsign = "JSmith" },
            new Airsofter { Id = Guid.NewGuid(), FirstName = "Bob", LastName = "Brown", Callsign = "BBrown" }
        );
        await _context.SaveChangesAsync();

        var handler = new GetAirsoftersListQueryHandler(_context, _mapper);
        var result = await handler.Handle(
            new GetAirsoftersListQuery { Callsign = "Smith" }, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Callsign.Should().Be("JSmith");
    }
}
