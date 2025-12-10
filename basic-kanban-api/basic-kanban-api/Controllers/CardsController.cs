using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
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
    public class CardsController : ControllerBase
    {
        private readonly KanbanDbContext _context;
        private readonly UserManager<User> _userManager;

        public CardsController(KanbanDbContext context, UserManager<User> userManager)
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

        private CardDto MapToDto(Card card)
        {
            return new CardDto
            {
                Id = card.Id,
                Title = card.Title,
                Description = card.Description,
                Difficulty = (int)card.Difficulty,
                EstimatedStart = card.EstimatedStart,
                EstimatedEnd = card.EstimatedEnd,
                ActualStart = card.ActualStart,
                ActualEnd = card.ActualEnd,
                CardListId = card.CardListId,
                AssignedToUserId = card.AssignedToUserId,
                AssignedToUserName = card.AssignedToUser?.FullName,
                CreatedAt = card.CreatedAt,
                UpdatedAt = card.UpdatedAt,
                Order = card.Order
            };
        }

        // GET: api/cards/cardlist/{cardListId}
        [HttpGet("cardlist/{cardListId}")]
        public async Task<ActionResult<IEnumerable<CardDto>>> GetCardsByCardList(Guid cardListId)
        {
            var cardList = await _context.CardLists
                .Include(cl => cl.Board)
                .FirstOrDefaultAsync(cl => cl.Id == cardListId);

            if (cardList == null)
                return NotFound();

            var user = await GetCurrentUserAsync();
            if (!await IsUserBoardMemberAsync(cardList.BoardId, user))
                return Forbid();

            var cards = await _context.Cards
                .Where(c => c.CardListId == cardListId)
                .Include(c => c.AssignedToUser)
                .OrderBy(c => c.Order)
                .ToListAsync();

            return Ok(cards.Select(MapToDto).ToList());
        }

        // GET: api/cards/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CardDto>> GetCard(Guid id)
        {
            var card = await _context.Cards
                .Include(c => c.CardList)
                .Include(c => c.AssignedToUser)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (card == null)
                return NotFound();

            var user = await GetCurrentUserAsync();
            if (!await IsUserBoardMemberAsync(card.CardList.BoardId, user))
                return Forbid();

            return Ok(MapToDto(card));
        }

        // POST: api/cards
        [HttpPost]
        public async Task<ActionResult<CardDto>> PostCard(CreateCardDto createCardDto)
        {
            var cardList = await _context.CardLists
                .Include(cl => cl.Board)
                .FirstOrDefaultAsync(cl => cl.Id == createCardDto.CardListId);

            if (cardList == null)
                return BadRequest("CardList not found");

            var user = await GetCurrentUserAsync();
            if (!await IsUserBoardMemberAsync(cardList.BoardId, user))
                return Forbid();

            var card = new Card
            {
                Id = Guid.NewGuid(),
                Title = createCardDto.Title,
                Description = createCardDto.Description,
                Difficulty = (Category.CategoryType)createCardDto.Difficulty,
                EstimatedStart = createCardDto.EstimatedStart,
                EstimatedEnd = createCardDto.EstimatedEnd,
                CardListId = createCardDto.CardListId,
                AssignedToUserId = createCardDto.AssignedToUserId,
                CreatedAt = DateTime.UtcNow,
                Order = await _context.Cards
                    .Where(c => c.CardListId == createCardDto.CardListId)
                    .MaxAsync(c => (int?)c.Order) ?? 0 + 1
            };

            _context.Cards.Add(card);
            await _context.SaveChangesAsync();

            await _context.Entry(card).Reference(c => c.AssignedToUser).LoadAsync();

            return CreatedAtAction("GetCard", new { id = card.Id }, MapToDto(card));
        }

        // PUT: api/cards/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCard(Guid id, UpdateCardDto updateCardDto)
        {
            var card = await _context.Cards
                .Include(c => c.CardList)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (card == null)
                return NotFound();

            var user = await GetCurrentUserAsync();
            if (!await IsUserBoardMemberAsync(card.CardList.BoardId, user))
                return Forbid();

            card.Title = updateCardDto.Title ?? card.Title;
            card.Description = updateCardDto.Description ?? card.Description;
            card.Difficulty = (Category.CategoryType)updateCardDto.Difficulty;
            card.EstimatedStart = updateCardDto.EstimatedStart ?? card.EstimatedStart;
            card.EstimatedEnd = updateCardDto.EstimatedEnd ?? card.EstimatedEnd;
            card.ActualStart = updateCardDto.ActualStart ?? card.ActualStart;
            card.ActualEnd = updateCardDto.ActualEnd ?? card.ActualEnd;
            card.AssignedToUserId = updateCardDto.AssignedToUserId ?? card.AssignedToUserId;
            card.Order = updateCardDto.Order ?? card.Order;
            card.UpdatedAt = DateTime.UtcNow;

            _context.Entry(card).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            await _context.Entry(card).Reference(c => c.AssignedToUser).LoadAsync();

            return Ok(MapToDto(card));
        }

        // DELETE: api/cards/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCard(Guid id)
        {
            var card = await _context.Cards
                .Include(c => c.CardList)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (card == null)
                return NotFound();

            var user = await GetCurrentUserAsync();
            if (!await IsUserBoardMemberAsync(card.CardList.BoardId, user))
                return Forbid();

            _context.Cards.Remove(card);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
