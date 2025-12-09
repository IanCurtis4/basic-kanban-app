using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static basic_kanban_api.Models.Category;

namespace basic_kanban_api.Models
{
    public class Card
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [EnumDataType(typeof(CategoryType))]
        public CategoryType Difficulty { get; set; }

        public (DateTime start, DateTime end) EstimatedTime { get; set; }

        public (DateTime start, DateTime end) ActualTime { get; set; }

        // Relacionamentos
        [Required]
        [ForeignKey(nameof(CardList))]
        public Guid CardListId { get; set; }

        public Guid? AssignedToUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public int Order { get; set; }

        public CardList CardList { get; set; }

        public User AssignedToUser { get; set; }
    }
}
