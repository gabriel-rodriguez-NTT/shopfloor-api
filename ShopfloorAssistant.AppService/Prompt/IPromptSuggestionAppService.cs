using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShopfloorAssistant.AppService
{
    public interface IPromptSuggestionAppService
    {
        Task<IEnumerable<PromptSuggestionDto>> GetAllAsync();
        Task<PromptSuggestionDto?> GetByIdAsync(Guid id);
        Task<PromptSuggestionDto> CreateAsync(PromptSuggestionCreateDto dto);
        Task UpdateAsync(Guid id, PromptSuggestionUpdateDto dto);
        Task DeleteAsync(Guid id);
    }
}
