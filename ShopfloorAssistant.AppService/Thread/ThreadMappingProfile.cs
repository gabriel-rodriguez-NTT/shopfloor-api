using AutoMapper;
using ShopfloorAssistant.Core.Entities;

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
            }
        }

}
