using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using basic_kanban_api.Data;
using basic_kanban_api.DTOs;
using basic_kanban_api.Models;

namespace basic_kanban_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BoardsController : ControllerBase
    {
        private readonly KanbanDbContext _context;
        private readonly UserManager<User> _userManager;

        public BoardsController(KanbanDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<User> GetCurrentUserAsync()
        {
            return await _userManager.GetUserAsync(User);
        }

        private BoardDto MapToDto(Board board)
        {
            return new BoardDto
            {
                Id = board.Id,
                Title = board.Title,
                Description = board.Description,
                OwnerId = board.OwnerId,
                OwnerName = board.Owner?.FullName,
                CreatedAt = board.CreatedAt,
                UpdatedAt = board.UpdatedAt,
                IsArchived = board.IsArchived,
                CardLists = board.CardLists?.Select(cl => new CardListDto
                {
                    Id = cl.Id,
                    Title = cl.Title,
                    Description = cl.Description,
                    BoardId = cl.BoardId,
                    Order = cl.Order,
                    CreatedAt = cl.CreatedAt,
                    UpdatedAt = cl.UpdatedAt,
                    Cards = cl.Cards?.Select(c => new CardDto
                    {
                        Id = c.Id,
                        Title = c.Title,
                        Description = c.Description,
                        Difficulty = (int)c.Difficulty,
                        CardListId = c.CardListId,
                        AssignedToUserId = c.AssignedToUserId,
                        AssignedToUserName = c.AssignedToUser?.FullName,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        Order = c.Order
                    }).ToList() ?? new List<CardDto>()
                }).ToList() ?? new List<CardListDto>(),
                Members = board.Members?.Select(bm => new BoardMemberDto
                {
                    Id = bm.Id,
                    UserId = bm.UserId,
                    UserName = bm.User?.UserName,
                    UserFullName = bm.User?.FullName,
                    Role = bm.Role.ToString(),
                    JoinedAt = bm.JoinedAt
                }).ToList() ?? new List<BoardMemberDto>()
            };
        }

        // GET: api/boards
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BoardDto>>> GetBoards()
        {
            var user = await GetCurrentUserAsync();

            var boards = await _context.Boards
                .Include(b => b.Owner)
                .Include(b => b.Members)
                    .ThenInclude(bm => bm.User)
                .Include(b => b.CardLists)
                    .ThenInclude(cl => cl.Cards)
                        .ThenInclude(c => c.AssignedToUser)
                .Where(b => b.OwnerId == user.Id || b.Members.Any(m => m.UserId == user.Id))
                .ToListAsync();

            return Ok(boards.Select(MapToDto).ToList());
        }

        // GET: api/boards/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BoardDto>> GetBoard(Guid id)
        {
            var board = await _context.Boards
                .Include(b => b.Owner)
                .Include(b => b.Members)
                    .ThenInclude(bm => bm.User)
                .Include(b => b.CardLists)
                    .ThenInclude(cl => cl.Cards)
                        .ThenInclude(c => c.AssignedToUser)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (board == null)
                return NotFound();

            var user = await GetCurrentUserAsync();
            var isMember = board.OwnerId == user.Id || board.Members.Any(m => m.UserId == user.Id);

            if (!isMember)
                return Forbid();

            return Ok(MapToDto(board));
        }

        // POST: api/boards
        [HttpPost]
        public async Task<ActionResult<BoardDto>> PostBoard(CreateBoardDto createBoardDto)
        {
            var user = await GetCurrentUserAsync();

            var board = new Board
            {
                Id = Guid.NewGuid(),
                Title = createBoardDto.Title,
                Description = createBoardDto.Description,
                OwnerId = user.Id,
                Owner = user,
                CreatedAt = DateTime.UtcNow
            };

            _context.Boards.Add(board);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBoard", new { id = board.Id }, MapToDto(board));
        }

        // PUT: api/boards/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBoard(Guid id, UpdateBoardDto updateBoardDto)
        {
            var board = await _context.Boards.FirstOrDefaultAsync(b => b.Id == id);

            if (board == null)
                return NotFound();

            var user = await GetCurrentUserAsync();
            if (board.OwnerId != user.Id)
                return Forbid();

            board.Title = updateBoardDto.Title ?? board.Title;
            board.Description = updateBoardDto.Description ?? board.Description;
            board.IsArchived = updateBoardDto.IsArchived ?? board.IsArchived;
            board.UpdatedAt = DateTime.UtcNow;

            _context.Entry(board).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            await _context.Entry(board)
                .Collection(b => b.CardLists)
                .LoadAsync();

            await _context.Entry(board)
                .Collection(b => b.Members)
                .LoadAsync();

            return Ok(MapToDto(board));
        }

        // DELETE: api/boards/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBoard(Guid id)
        {
            var board = await _context.Boards.FirstOrDefaultAsync(b => b.Id == id);

            if (board == null)
                return NotFound();

            var user = await GetCurrentUserAsync();
            if (board.OwnerId != user.Id)
                return Forbid();

            _context.Boards.Remove(board);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/boards/5/members
        [HttpPost("{boardId}/members")]
        public async Task<ActionResult<BoardMemberDto>> AddBoardMember(Guid boardId, AddBoardMemberDto addMemberDto)
        {
            var board = await _context.Boards.FirstOrDefaultAsync(b => b.Id == boardId);

            if (board == null)
                return NotFound();

            var user = await GetCurrentUserAsync();
            if (board.OwnerId != user.Id)
                return Forbid();

            var memberToAdd = await _userManager.FindByIdAsync(addMemberDto.UserId.ToString());
            if (memberToAdd == null)
                return BadRequest("User not found");

            var existingMember = await _context.BoardMembers
                .FirstOrDefaultAsync(bm => bm.BoardId == boardId && bm.UserId == addMemberDto.UserId);

            if (existingMember != null)
                return BadRequest("User is already a member of this board");

            if (!Enum.TryParse<BoardMemberRole>(addMemberDto.Role, out var role))
                return BadRequest("Invalid role");

            var boardMember = new BoardMember
            {
                Id = Guid.NewGuid(),
                BoardId = boardId,
                UserId = addMemberDto.UserId,
                Role = role,
                JoinedAt = DateTime.UtcNow
            };

            _context.BoardMembers.Add(boardMember);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBoardMember", new { boardId, memberId = boardMember.Id }, new BoardMemberDto
            {
                Id = boardMember.Id,
                UserId = boardMember.UserId,
                UserName = memberToAdd.UserName,
                UserFullName = memberToAdd.FullName,
                Role = boardMember.Role.ToString(),
                JoinedAt = boardMember.JoinedAt
            });
        }

        // DELETE: api/boards/5/members/memberId
        [HttpDelete("{boardId}/members/{memberId}")]
        public async Task<IActionResult> RemoveBoardMember(Guid boardId, Guid memberId)
        {
            var board = await _context.Boards.FirstOrDefaultAsync(b => b.Id == boardId);

            if (board == null)
                return NotFound();

            var user = await GetCurrentUserAsync();
            if (board.OwnerId != user.Id)
                return Forbid();

            var member = await _context.BoardMembers
                .FirstOrDefaultAsync(bm => bm.Id == memberId && bm.BoardId == boardId);

            if (member == null)
                return NotFound();

            _context.BoardMembers.Remove(member);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
