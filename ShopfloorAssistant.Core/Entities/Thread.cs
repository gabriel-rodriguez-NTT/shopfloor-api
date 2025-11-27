using System;

namespace ShopfloorAssistant.Core.Entities
{
    public class Thread : AuditableEntity
    {
        public Guid Id { get; set; }
        public string User { get; set; }
        public virtual ICollection<ThreadMessage> Messages { get; set; }
    }
}
