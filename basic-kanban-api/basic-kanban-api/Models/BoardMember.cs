using System.ComponentModel.DataAnnotations;

namespace basic_kanban_api.Models
{
    public enum BoardMemberRole
    {
        Owner,
        Manager,
        Editor,
        Viewer
    }

    public class BoardMember
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid BoardId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public BoardMemberRole Role { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Relacionamentos
        public Board Board { get; set; }
        public User User { get; set; }
    }
}
