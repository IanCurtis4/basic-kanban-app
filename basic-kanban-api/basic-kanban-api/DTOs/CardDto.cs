namespace basic_kanban_api.DTOs
{
    public class CardDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Difficulty { get; set; }
        public DateTime? EstimatedStart { get; set; }
        public DateTime? EstimatedEnd { get; set; }
        public DateTime? ActualStart { get; set; }
        public DateTime? ActualEnd { get; set; }
        public Guid CardListId { get; set; }
        public Guid? AssignedToUserId { get; set; }
        public string AssignedToUserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int Order { get; set; }
    }

    public class CreateCardDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Difficulty { get; set; }
        public DateTime? EstimatedStart { get; set; }
        public DateTime? EstimatedEnd { get; set; }
        public Guid CardListId { get; set; }
        public Guid? AssignedToUserId { get; set; }
    }

    public class UpdateCardDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Difficulty { get; set; }
        public DateTime? EstimatedStart { get; set; }
        public DateTime? EstimatedEnd { get; set; }
        public DateTime? ActualStart { get; set; }
        public DateTime? ActualEnd { get; set; }
        public Guid? AssignedToUserId { get; set; }
        public int? Order { get; set; }
    }
}
