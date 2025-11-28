namespace ShopfloorAssistant.AppService
{
    public abstract class AuditableDto
    {
        public DateTime CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }
    }
}
