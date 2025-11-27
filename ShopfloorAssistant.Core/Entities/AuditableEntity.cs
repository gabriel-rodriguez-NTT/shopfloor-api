namespace ShopfloorAssistant.Core.Entities
{
    public abstract class AuditableEntity
    {
        public DateTime CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }
    }

}
