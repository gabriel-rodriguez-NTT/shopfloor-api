using AutoMapper;
using ShopfloorAssistant.Core.Entities;
using ShopfloorAssistant.Core.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShopfloorAssistant.AppService
{
    public class PromptSuggestionAppService : IPromptSuggestionAppService
    {
        private readonly IGenericRepository<PromptSuggestion> _repository;
        private readonly IMapper _mapper;

        public PromptSuggestionAppService(IGenericRepository<PromptSuggestion> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PromptSuggestionDto>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<PromptSuggestionDto>>(entities);
        }

        public async Task<PromptSuggestionDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return null;
            return _mapper.Map<PromptSuggestionDto>(entity);
        }

        public async Task<PromptSuggestionDto> CreateAsync(PromptSuggestionCreateDto dto)
        {
            var entity = _mapper.Map<PromptSuggestion>(dto);
            entity.Id = Guid.NewGuid();
            entity.CreationTime = DateTime.UtcNow;

            await _repository.AddAsync(entity);

            return _mapper.Map<PromptSuggestionDto>(entity);
        }

        public async Task UpdateAsync(Guid id, PromptSuggestionUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) throw new KeyNotFoundException($"PromptSuggestion with id {id} not found.");

            _mapper.Map(dto, entity);
            entity.LastModificationTime = DateTime.UtcNow;

            await _repository.UpdateAsync(entity);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}
