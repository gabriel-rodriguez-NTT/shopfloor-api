using AutoMapper;
using ShopfloorAssistant.Core.Entities;
using System.Text.Json;

namespace ShopfloorAssistant.AppService
{
    public class PromptMappingProfile : Profile
    {
        public PromptMappingProfile()
        {
            CreateMap<PromptSuggestion, PromptSuggestionDto>()
                //.ForMember(dest => dest.Metadata, opt => opt.MapFrom<MetadataToStringResolver>())
                ;

            CreateMap<PromptSuggestionCreateDto, PromptSuggestion>()
                //.ForMember(dest => dest.Metadata, opt => opt.MapFrom<StringToMetadataCreateResolver>())
                ;

            CreateMap<PromptSuggestionUpdateDto, PromptSuggestion>()
                //.ForMember(dest => dest.Metadata, opt => opt.MapFrom<StringToMetadataUpdateResolver>())
                ;
        }
    }

    // Resolver: converts IDictionary<string, object> -> JSON string
    public class MetadataToStringResolver : IValueResolver<PromptSuggestion, PromptSuggestionDto, string?>
    {
        public string? Resolve(PromptSuggestion source, PromptSuggestionDto destination, string? destMember, ResolutionContext context)
        {
            if (source?.Metadata == null) return null;
            return JsonSerializer.Serialize(source.Metadata);
        }
    }

    // Resolver: converts JSON string -> IDictionary<string, object> for CreateDto
    public class StringToMetadataCreateResolver : IValueResolver<PromptSuggestionCreateDto, PromptSuggestion, IDictionary<string, object>?>
    {
        public IDictionary<string, object>? Resolve(PromptSuggestionCreateDto source, PromptSuggestion destination, IDictionary<string, object>? destMember, ResolutionContext context)
        {
            if (string.IsNullOrWhiteSpace(source?.Metadata)) return null;
            return JsonSerializer.Deserialize<Dictionary<string, object>>(source.Metadata);
        }
    }

    // Resolver: converts JSON string -> IDictionary<string, object> for UpdateDto
    public class StringToMetadataUpdateResolver : IValueResolver<PromptSuggestionUpdateDto, PromptSuggestion, IDictionary<string, object>?>
    {
        public IDictionary<string, object>? Resolve(PromptSuggestionUpdateDto source, PromptSuggestion destination, IDictionary<string, object>? destMember, ResolutionContext context)
        {
            if (string.IsNullOrWhiteSpace(source?.Metadata)) return null;
            return JsonSerializer.Deserialize<Dictionary<string, object>>(source.Metadata);
        }
    }
}
