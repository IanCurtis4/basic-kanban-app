namespace basic_kanban_api.DTOs
{
    public class CardListDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid BoardId { get; set; }
        public int Order { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ICollection<CardDto> Cards { get; set; } = new List<CardDto>();
    }

    public class CreateCardListDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid BoardId { get; set; }
    }

    public class UpdateCardListDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int? Order { get; set; }
    }
}
