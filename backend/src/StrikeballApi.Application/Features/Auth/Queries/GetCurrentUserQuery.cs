using MediatR;
using StrikeballApi.Application.DTOs.Auth;

namespace StrikeballApi.Application.Features.Auth.Queries;

public class GetCurrentUserQuery : IRequest<UserInfo>
{
    public Guid UserId { get; set; }
}
