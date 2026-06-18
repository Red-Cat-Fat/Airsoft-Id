using MediatR;
using StrikeballApi.Application.Common.Interfaces;
using StrikeballApi.Application.DTOs.Auth;

namespace StrikeballApi.Application.Features.Auth.Queries;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserInfo>
{
    private readonly ICurrentUserService _currentUser;

    public GetCurrentUserQueryHandler(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public async Task<UserInfo> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
        {
            throw new Common.Exceptions.UnauthorizedException();
        }

        return await Task.FromResult(new UserInfo
        {
            Id = request.UserId,
            Email = _currentUser.UserName ?? string.Empty,
            Roles = new List<string>()
        });
    }
}
