using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace basic_kanban_api.Models
{
    public class Board
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        [ForeignKey(nameof(Owner))]
        public Guid OwnerId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsArchived { get; set; } = false;

        // Relacionamentos
        public User Owner { get; set; }

        public ICollection<CardList> CardLists { get; set; } = new List<CardList>();

        public ICollection<BoardMember> Members { get; set; } = new List<BoardMember>();
    }
}
