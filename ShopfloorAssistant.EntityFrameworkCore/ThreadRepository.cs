using Microsoft.EntityFrameworkCore;
using ShopfloorAssistant.Core.Entities;
using ShopfloorAssistant.Core.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace ShopfloorAssistant.EntityFrameworkCore
{
    public class ThreadRepository : GenericRepository<Thread>, IThreadRepository
    {
        public ThreadRepository(ShopfloorAssistantDbContext context) : base(context)
        {
        }

        public async Task AddMessagesAsync(Guid threadId, string user, IEnumerable<ThreadMessage> messages)
        {
            // Cargar el thread con sus mensajes (solo Ids para no gastar memoria)
            var existingThread = await _context.Threads
                .Include(t => t.Messages)
                .FirstOrDefaultAsync(t => t.Id == threadId);

            if (existingThread == null)
            {
                // Crear nuevo thread
                var newThread = new Thread
                {
                    Id = threadId,
                    User = user,
                    Messages = messages.ToList()
                };

                await _context.Threads.AddAsync(newThread);
            }
            else
            {
                // Filtrar mensajes ya existentes
                var existingMessageIds = existingThread.Messages
                    .Select(m => m.Id)
                    .ToHashSet();

                var newMessages = messages
                    .Where(m => !existingMessageIds.Contains(m.Id))
                    .ToList();

                if (newMessages.Any())
                {
                    foreach (var msg in newMessages)
                        existingThread.Messages.Add(msg);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Thread>> GetUserThreads(string user)
        {
            return await _context.Threads.Where(t => t.User == user)
                .Include(t => t.Messages)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Thread> GetByIdAsync(Guid id)
        {
            return await _context.Threads
                .Include(t => t.Messages)
                .ThenInclude(t => t.ToolCalls)
                .FirstAsync(x => x.Id == id);
        }
    }
}
