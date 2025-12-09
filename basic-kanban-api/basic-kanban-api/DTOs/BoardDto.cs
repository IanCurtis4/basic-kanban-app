namespace basic_kanban_api.DTOs
{
    public class BoardDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid OwnerId { get; set; }
        public string OwnerName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsArchived { get; set; }
        public ICollection<CardListDto> CardLists { get; set; } = new List<CardListDto>();
        public ICollection<BoardMemberDto> Members { get; set; } = new List<BoardMemberDto>();
    }

    public class CreateBoardDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public class UpdateBoardDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public bool? IsArchived { get; set; }
    }

    public class BoardMemberDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string UserFullName { get; set; }
        public string Role { get; set; }
        public DateTime JoinedAt { get; set; }
    }

    public class AddBoardMemberDto
    {
        public Guid UserId { get; set; }
        public string Role { get; set; }
    }
}
