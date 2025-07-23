using AutoMapper;
using AIAgentService.Application.DTOs;
using AIAgentService.Domain.Entities;

namespace AIAgentService.Application.Mappings
{
    public class AgentMappingProfile : Profile
    {
        public AgentMappingProfile()
        {
            CreateMap<AgentSession, AgentSessionDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<AgentMessage, AgentMessageDto>()
                .ForMember(dest => dest.MessageType, opt => opt.MapFrom(src => src.MessageType.ToString()));

            CreateMap<AgentSkill, AgentSkillDto>();

            // Reverse mappings if needed
            CreateMap<AgentSessionDto, AgentSession>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<AgentSessionStatus>(src.Status)))
                .ConstructUsing((src, context) => new AgentSession(src.UserId, src.SessionName, src.Description, src.Context));

            CreateMap<AgentSkillDto, AgentSkill>()
                .ConstructUsing((src, context) => new AgentSkill(
                    src.Name, 
                    src.Description, 
                    src.ServiceName, 
                    src.SkillType, 
                    src.Configuration, 
                    src.Version));
        }
    }
}
