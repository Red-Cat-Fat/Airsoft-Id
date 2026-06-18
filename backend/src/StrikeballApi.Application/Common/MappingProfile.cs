using AutoMapper;
using StrikeballApi.Application.DTOs.Airsofters;
using StrikeballApi.Domain.Entities;

namespace StrikeballApi.Application.Common;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Airsofter, AirsofterDto>()
            .ForMember(d => d.TeamName, opt => opt.MapFrom(s => s.Team != null ? s.Team.FullName : null))
            .ForMember(d => d.GamesPlayed, opt => opt.MapFrom(s => s.GameParticipants != null ? s.GameParticipants.Count : 0))
            .ForMember(d => d.ViolationsCount, opt => opt.MapFrom(s => s.Violations != null ? s.Violations.Count : 0));

        CreateMap<Airsofter, AirsofterDetailsDto>()
            .ForMember(d => d.TeamName, opt => opt.MapFrom(s => s.Team != null ? s.Team.FullName : null))
            .ForMember(d => d.GamesPlayed, opt => opt.MapFrom(s => s.GameParticipants != null ? s.GameParticipants.Count : 0))
            .ForMember(d => d.ViolationsCount, opt => opt.MapFrom(s => s.Violations != null ? s.Violations.Count : 0))
            .ForMember(d => d.GameHistory, opt => opt.MapFrom(s => s.GameParticipants))
            .ForMember(d => d.Violations, opt => opt.MapFrom(s => s.Violations));

        CreateMap<GameParticipant, GameHistoryDto>()
            .ForMember(d => d.GameId, opt => opt.MapFrom(s => s.GameId))
            .ForMember(d => d.Date, opt => opt.MapFrom(s => s.Game != null ? s.Game.Date : default(DateTime)))
            .ForMember(d => d.FieldName, opt => opt.MapFrom(s => s.Game != null && s.Game.Field != null ? s.Game.Field.Name : null))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        CreateMap<Violation, ViolationDto>()
            .ForMember(d => d.FieldName, opt => opt.MapFrom(s => s.Field != null ? s.Field.Name : null))
            .ForMember(d => d.Severity, opt => opt.MapFrom(s => s.Severity.ToString()));
    }
}
