using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace basic_kanban_api.Models
{
    public class CardList
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [ForeignKey(nameof(Board))]
        public Guid BoardId { get; set; }

        public int Order { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Relacionamentos
        public Board Board { get; set; }

        public ICollection<Card> Cards { get; set; } = new List<Card>();
    }
}
