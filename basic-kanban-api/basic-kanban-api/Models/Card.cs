using System.ComponentModel.DataAnnotations;
using static basic_kanban_api.Models.Category;

namespace basic_kanban_api.Models
{
    public class Card
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        [EnumDataType(typeof(CategoryType))]
        public CategoryType Difficulty { get; set; }
        public (DateTime start, DateTime end) EstimatedTime { get; set; }
        public (DateTime start, DateTime end) ActualTime { get; set; }
    }
}
