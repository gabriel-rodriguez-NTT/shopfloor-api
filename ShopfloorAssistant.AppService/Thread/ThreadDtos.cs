using Microsoft.Extensions.AI;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ShopfloorAssistant.AppService
{
    public class ThreadDto : AuditableDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string? Title { get; set; }
        //public List<ThreadMessageDto> Messages { get; set; } = new();
    }
}
