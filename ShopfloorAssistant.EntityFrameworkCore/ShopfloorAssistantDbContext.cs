using Microsoft.EntityFrameworkCore;
using ShopfloorAssistant.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Thread = ShopfloorAssistant.Core.Entities.Thread;

namespace ShopfloorAssistant.EntityFrameworkCore
{
    public class ShopfloorAssistantDbContext : DbContext
    {
        public DbSet<Thread> Threads { get; set; }
        public DbSet<ThreadMessage> ThreadMessages { get; set; }

        // Nuevo DbSet
        public DbSet<ThreadToolCall> ThreadToolCalls { get; set; }

        public ShopfloorAssistantDbContext(DbContextOptions<ShopfloorAssistantDbContext> dbContextOptions)
            : base(dbContextOptions)
        {
        }

        public override int SaveChanges()
        {
            ApplyAuditInfo();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditInfo();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyAuditInfo()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is AuditableEntity &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (AuditableEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.CreationTime = DateTime.UtcNow;
                }

                entity.LastModificationTime = DateTime.UtcNow;
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // JsonSerializer options explícitas (evita CS0854)
            var jsonOptions = new System.Text.Json.JsonSerializerOptions();

            // Thread
            modelBuilder.Entity<Thread>()
                .HasKey(t => t.Id);

            // ThreadMessage
            modelBuilder.Entity<ThreadMessage>()
                .HasKey(tm => tm.Id);

            modelBuilder.Entity<ThreadMessage>()
                .HasOne(tm => tm.Thread)
                .WithMany(t => t.Messages)
                .HasForeignKey(tm => tm.ThreadId);

            modelBuilder.Entity<ThreadToolCall>()
                .HasKey(tc => tc.Id);

            modelBuilder.Entity<ThreadToolCall>()
                .HasOne(tc => tc.ThreadMessage)
                .WithMany(tm => tm.ToolCalls)
                .HasForeignKey(tc => tc.ThreadMessageId);

            modelBuilder.Entity<ThreadToolCall>()
                .Property(tc => tc.Arguments)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, jsonOptions),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, jsonOptions)
                );
        }

    }
}
