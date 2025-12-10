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
    public class CardListsController : ControllerBase
    {
        private readonly KanbanDbContext _context;
        private readonly UserManager<User> _userManager;

        public CardListsController(KanbanDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<User> GetCurrentUserAsync()
        {
            return await _userManager.GetUserAsync(User);
        }

        private async Task<bool> IsUserBoardMemberAsync(Guid boardId, User user)
        {
            var board = await _context.Boards.FirstOrDefaultAsync(b => b.Id == boardId);
            if (board == null) return false;

            if (board.OwnerId == user.Id) return true;

            return await _context.BoardMembers
                .AnyAsync(bm => bm.BoardId == boardId && bm.UserId == user.Id);
        }

        private CardListDto MapToDto(CardList cardList)
        {
            return new CardListDto
            {
                Id = cardList.Id,
                Title = cardList.Title,
                Description = cardList.Description,
                BoardId = cardList.BoardId,
                Order = cardList.Order,
                CreatedAt = cardList.CreatedAt,
                UpdatedAt = cardList.UpdatedAt,
                Cards = cardList.Cards?.Select(c => new CardDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Difficulty = (int)c.Difficulty,
                    EstimatedStart = c.EstimatedStart,
                    EstimatedEnd = c.EstimatedEnd,
                    ActualStart = c.ActualStart,
                    ActualEnd = c.ActualEnd,
                    CardListId = c.CardListId,
                    AssignedToUserId = c.AssignedToUserId,
                    AssignedToUserName = c.AssignedToUser?.FullName,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    Order = c.Order
                }).ToList() ?? new List<CardDto>()
            };
        }

        // GET: api/cardlists/board/5
        [HttpGet("board/{boardId}")]
        public async Task<ActionResult<IEnumerable<CardListDto>>> GetCardListsByBoard(Guid boardId)
        {
            var user = await GetCurrentUserAsync();
            if (!await IsUserBoardMemberAsync(boardId, user))
                return Forbid();

            var cardLists = await _context.CardLists
                .Where(cl => cl.BoardId == boardId)
                .Include(cl => cl.Cards)
                    .ThenInclude(c => c.AssignedToUser)
                .OrderBy(cl => cl.Order)
                .ToListAsync();

            return Ok(cardLists.Select(MapToDto).ToList());
        }

        // GET: api/cardlists/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CardListDto>> GetCardList(Guid id)
        {
            var cardList = await _context.CardLists
                .Include(cl => cl.Cards)
                    .ThenInclude(c => c.AssignedToUser)
                .Include(cl => cl.Board)
                .FirstOrDefaultAsync(cl => cl.Id == id);

            if (cardList == null)
                return NotFound();

            var user = await GetCurrentUserAsync();
            if (!await IsUserBoardMemberAsync(cardList.BoardId, user))
                return Forbid();

            return Ok(MapToDto(cardList));
        }

        // POST: api/cardlists
        [HttpPost]
        public async Task<ActionResult<CardListDto>> PostCardList(CreateCardListDto createCardListDto)
        {
            var board = await _context.Boards.FirstOrDefaultAsync(b => b.Id == createCardListDto.BoardId);

            if (board == null)
                return BadRequest("Board not found");

            var user = await GetCurrentUserAsync();
            if (!await IsUserBoardMemberAsync(createCardListDto.BoardId, user))
                return Forbid();

            var cardList = new CardList
            {
                Id = Guid.NewGuid(),
                Title = createCardListDto.Title,
                Description = createCardListDto.Description,
                BoardId = createCardListDto.BoardId,
                CreatedAt = DateTime.UtcNow,
                Order = await _context.CardLists
                    .Where(cl => cl.BoardId == createCardListDto.BoardId)
                    .MaxAsync(cl => (int?)cl.Order) ?? 0 + 1
            };

            _context.CardLists.Add(cardList);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCardList", new { id = cardList.Id }, MapToDto(cardList));
        }

        // PUT: api/cardlists/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCardList(Guid id, UpdateCardListDto updateCardListDto)
        {
            var cardList = await _context.CardLists
                .Include(cl => cl.Board)
                .FirstOrDefaultAsync(cl => cl.Id == id);

            if (cardList == null)
                return NotFound();

            var user = await GetCurrentUserAsync();
            if (!await IsUserBoardMemberAsync(cardList.BoardId, user))
                return Forbid();

            cardList.Title = updateCardListDto.Title ?? cardList.Title;
            cardList.Description = updateCardListDto.Description ?? cardList.Description;
            cardList.Order = updateCardListDto.Order ?? cardList.Order;
            cardList.UpdatedAt = DateTime.UtcNow;

            _context.Entry(cardList).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            await _context.Entry(cardList).Collection(cl => cl.Cards).LoadAsync();

            return Ok(MapToDto(cardList));
        }

        // DELETE: api/cardlists/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCardList(Guid id)
        {
            var cardList = await _context.CardLists
                .Include(cl => cl.Board)
                .FirstOrDefaultAsync(cl => cl.Id == id);

            if (cardList == null)
                return NotFound();

            var user = await GetCurrentUserAsync();
            if (!await IsUserBoardMemberAsync(cardList.BoardId, user))
                return Forbid();

            _context.CardLists.Remove(cardList);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/cardlists/reorder
        [HttpPost("reorder")]
        public async Task<IActionResult> ReorderCardLists([FromBody] ReorderDto[] reorderItems)
        {
            if (reorderItems == null || reorderItems.Length == 0)
                return BadRequest("No items to reorder");

            var user = await GetCurrentUserAsync();

            foreach (var item in reorderItems)
            {
                var cardList = await _context.CardLists
                    .Include(cl => cl.Board)
                    .FirstOrDefaultAsync(cl => cl.Id == item.Id);

                if (cardList == null)
                    return BadRequest($"CardList {item.Id} not found");

                if (!await IsUserBoardMemberAsync(cardList.BoardId, user))
                    return Forbid();

                cardList.Order = item.Order;
            }

            await _context.SaveChangesAsync();

            return Ok();
        }
    }

    public class ReorderDto
    {
        public Guid Id { get; set; }
        public int Order { get; set; }
    }
}
