using Microsoft.AspNetCore.Identity;

namespace basic_kanban_api.Models
{
    public class Role : IdentityRole<Guid>
    {
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
