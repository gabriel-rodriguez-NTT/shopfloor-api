using Microsoft.EntityFrameworkCore;
using ShopfloorAssistant.Core.Entities;
using System;
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

        public ShopfloorAssistantDbContext(DbContextOptions<ShopfloorAssistantDbContext> dbContextOptions) : base(dbContextOptions)
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
            modelBuilder.Entity<Thread>()
                .HasKey(ut => ut.Id);
            modelBuilder.Entity<ThreadMessage>()
                .HasKey(tm => tm.Id);

            modelBuilder.Entity<ThreadMessage>()
                .HasOne<Thread>(t => t.Thread)
                .WithMany(t => t.Messages)
                .HasForeignKey(tm => tm.ThreadId);

        }
    }
}
