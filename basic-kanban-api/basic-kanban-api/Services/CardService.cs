using Microsoft.EntityFrameworkCore;
using basic_kanban_api.Data;
using basic_kanban_api.Models;
using static basic_kanban_api.Models.Category;

namespace basic_kanban_api.Services
{
    public interface ICardService
    {
        Task<Card> GetCardByIdAsync(Guid cardId);
        Task<IEnumerable<Card>> GetCardsByCardListAsync(Guid cardListId);
        Task<Card> CreateCardAsync(Guid cardListId, string title, string description, CategoryType difficulty, Guid? assignedToUserId = null);
        Task<Card> UpdateCardAsync(Guid cardId, string title, string description, CategoryType difficulty, Guid? assignedToUserId, (DateTime, DateTime)? estimatedTime, (DateTime, DateTime)? actualTime);
        Task DeleteCardAsync(Guid cardId);
        Task AssignCardAsync(Guid cardId, Guid userId);
        Task UnassignCardAsync(Guid cardId);
    }

    public class CardService : ICardService
    {
        private readonly KanbanDbContext _context;

        public CardService(KanbanDbContext context)
        {
            _context = context;
        }

        public async Task<Card> GetCardByIdAsync(Guid cardId)
        {
            return await _context.Cards
                .Include(c => c.CardList)
                .Include(c => c.AssignedToUser)
                .FirstOrDefaultAsync(c => c.Id == cardId);
        }

        public async Task<IEnumerable<Card>> GetCardsByCardListAsync(Guid cardListId)
        {
            return await _context.Cards
                .Where(c => c.CardListId == cardListId)
                .Include(c => c.AssignedToUser)
                .OrderBy(c => c.Order)
                .ToListAsync();
        }

        public async Task<Card> CreateCardAsync(Guid cardListId, string title, string description, CategoryType difficulty, Guid? assignedToUserId = null)
        {
            var cardList = await _context.CardLists.FirstOrDefaultAsync(cl => cl.Id == cardListId);
            if (cardList == null)
                throw new InvalidOperationException("CardList not found");

            var maxOrder = await _context.Cards
                .Where(c => c.CardListId == cardListId)
                .MaxAsync(c => (int?)c.Order) ?? 0;

            var card = new Card
            {
                Id = Guid.NewGuid(),
                CardListId = cardListId,
                Title = title,
                Description = description,
                Difficulty = difficulty,
                AssignedToUserId = assignedToUserId,
                CreatedAt = DateTime.UtcNow,
                Order = maxOrder + 1
            };

            _context.Cards.Add(card);
            await _context.SaveChangesAsync();

            return card;
        }

        public async Task<Card> UpdateCardAsync(Guid cardId, string title, string description, CategoryType difficulty, Guid? assignedToUserId, (DateTime, DateTime)? estimatedTime, (DateTime, DateTime)? actualTime)
        {
            var card = await _context.Cards.FirstOrDefaultAsync(c => c.Id == cardId);
            if (card == null)
                throw new InvalidOperationException("Card not found");

            card.Title = title ?? card.Title;
            card.Description = description ?? card.Description;
            card.Difficulty = difficulty;
            card.AssignedToUserId = assignedToUserId;
            card.EstimatedTime = estimatedTime ?? card.EstimatedTime;
            card.ActualTime = actualTime ?? card.ActualTime;
            card.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return card;
        }

        public async Task DeleteCardAsync(Guid cardId)
        {
            var card = await _context.Cards.FirstOrDefaultAsync(c => c.Id == cardId);
            if (card != null)
            {
                _context.Cards.Remove(card);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AssignCardAsync(Guid cardId, Guid userId)
        {
            var card = await _context.Cards.FirstOrDefaultAsync(c => c.Id == cardId);
            if (card != null)
            {
                card.AssignedToUserId = userId;
                card.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UnassignCardAsync(Guid cardId)
        {
            var card = await _context.Cards.FirstOrDefaultAsync(c => c.Id == cardId);
            if (card != null)
            {
                card.AssignedToUserId = null;
                card.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
