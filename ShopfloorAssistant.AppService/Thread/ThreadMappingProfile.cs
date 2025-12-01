using AutoMapper;
using ShopfloorAssistant.Core.Entities;
using System.Text.Json;

namespace ShopfloorAssistant.AppService
{
    public class ThreadMappingProfile : Profile
    {
        public ThreadMappingProfile()
        {
            CreateMap<ShopfloorAssistant.Core.Entities.Thread, ThreadDto>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title));


            CreateMap<ThreadMessage, ThreadMessageDto>()
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Message))
                .ForMember(dest => dest.CreateAt, opt => opt.MapFrom(src => src.Timestamp));

            CreateMap<ThreadToolCall, ThreadCallDto>()
           .ForMember(dest => dest.Type, opt => opt.MapFrom(_ => "function"))
           .ForMember(dest => dest.Function, opt => opt.MapFrom(src => src));

            CreateMap<ThreadToolCall, ThreadFunctionCallDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Arguments, opt => opt.MapFrom(src =>
                    src.Arguments == null
                        ? "{}"
                        : JsonSerializer.Serialize(src.Arguments, new JsonSerializerOptions
                        {
                            WriteIndented = false
                        })
                ));
        }
    }

}
