using System;

namespace ShopfloorAssistant.Core.Entities
{
    public class Thread : AuditableEntity
    {
        public Guid Id { get; set; }
        public string User { get; set; }
        public virtual ICollection<ThreadMessage> Messages { get; set; }
        public string? Title
        {
            get
            {
                return Messages
                    .OrderBy(m => m.Order)      // ordenar por timestamp
                    .FirstOrDefault()?.Message;    // tomar el primer mensaje
            }
        }
    }
}
