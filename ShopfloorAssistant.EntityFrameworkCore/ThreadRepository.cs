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

        public async Task AddMessagesAsync(Guid threadId, string userEmail, IEnumerable<ThreadMessage> incomingMessages)
        {
            var thread = await _context.Threads
                .Include(t => t.Messages)
                    .ThenInclude(m => m.ToolCalls)
                .FirstOrDefaultAsync(t => t.Id == threadId);

            if (thread == null)
            {
                // Crear nuevo thread
                var newThread = new Thread
                {
                    Id = threadId,
                    User = userEmail,
                    CreationTime = DateTime.UtcNow,
                    Messages = incomingMessages.Select(m =>
                    {
                        m.ThreadId = threadId;
                        m.Thread = null;
                        foreach (var tc in m.ToolCalls)
                        {
                            tc.Id = Guid.NewGuid();
                            tc.ThreadMessageId = m.Id;
                            tc.ThreadMessage = null;
                        }
                        return m;
                    }).ToList(),
                    LastModificationTime = DateTime.UtcNow
                };

                await _context.Threads.AddAsync(newThread);
            }
            else
            {
                // Filtrar mensajes nuevos
                var existingIds = thread.Messages.Select(m => m.Id).ToHashSet();

                var newMessages = incomingMessages
                    .Where(m => !existingIds.Contains(m.Id))
                    .Select(m =>
                    {
                        m.ThreadId = threadId;
                        m.Thread = null;

                        // Asignar Id a ToolCalls y FK
                        foreach (var tc in m.ToolCalls)
                        {
                            //tc.Id = Guid.NewGuid();
                            tc.ThreadMessageId = m.Id;
                            tc.ThreadMessage = null;
                        }

                        return m;
                    })
                    .ToList();

                foreach (var msg in newMessages)
                {
                    thread.Messages.Add(msg);
                }

                // Actualizar LastModificationTime
                thread.LastModificationTime = DateTime.UtcNow;

                // Evitar que EF intente actualizar el thread completo
                //_context.Entry(thread).State = EntityState.Unchanged;
                //_context.Entry(thread).Property(t => t.LastModificationTime).IsModified = true;
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
