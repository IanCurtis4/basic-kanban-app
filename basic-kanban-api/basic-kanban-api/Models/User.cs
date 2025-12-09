using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace basic_kanban_api.Models
{
    public class User : IdentityUser<Guid>
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLogin { get; set; }

        public bool IsActive { get; set; } = true;

        // Relacionamentos
        public ICollection<Board> CreatedBoards { get; set; } = new List<Board>();
        public ICollection<BoardMember> BoardMemberships { get; set; } = new List<BoardMember>();
        public ICollection<Card> AssignedCards { get; set; } = new List<Card>();
    }
}
