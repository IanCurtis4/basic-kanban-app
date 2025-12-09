using Microsoft.EntityFrameworkCore;
using basic_kanban_api.Data;
using basic_kanban_api.Models;

namespace basic_kanban_api.Services
{
    public interface IBoardService
    {
        Task<Board> GetBoardByIdAsync(Guid boardId);
        Task<IEnumerable<Board>> GetUserBoardsAsync(Guid userId);
        Task<bool> IsUserBoardMemberAsync(Guid boardId, Guid userId);
        Task<bool> IsUserBoardOwnerAsync(Guid boardId, Guid userId);
        Task<BoardMember> AddBoardMemberAsync(Guid boardId, Guid userId, BoardMemberRole role);
        Task RemoveBoardMemberAsync(Guid boardId, Guid userId);
        Task UpdateBoardMemberRoleAsync(Guid boardId, Guid userId, BoardMemberRole role);
    }

    public class BoardService : IBoardService
    {
        private readonly KanbanDbContext _context;

        public BoardService(KanbanDbContext context)
        {
            _context = context;
        }

        public async Task<Board> GetBoardByIdAsync(Guid boardId)
        {
            return await _context.Boards
                .Include(b => b.Owner)
                .Include(b => b.Members)
                    .ThenInclude(bm => bm.User)
                .Include(b => b.CardLists)
                    .ThenInclude(cl => cl.Cards)
                .FirstOrDefaultAsync(b => b.Id == boardId);
        }

        public async Task<IEnumerable<Board>> GetUserBoardsAsync(Guid userId)
        {
            return await _context.Boards
                .Include(b => b.Owner)
                .Include(b => b.Members)
                    .ThenInclude(bm => bm.User)
                .Include(b => b.CardLists)
                    .ThenInclude(cl => cl.Cards)
                .Where(b => b.OwnerId == userId || b.Members.Any(m => m.UserId == userId))
                .ToListAsync();
        }

        public async Task<bool> IsUserBoardMemberAsync(Guid boardId, Guid userId)
        {
            var board = await _context.Boards
                .FirstOrDefaultAsync(b => b.Id == boardId);

            if (board == null) return false;
            if (board.OwnerId == userId) return true;

            return await _context.BoardMembers
                .AnyAsync(bm => bm.BoardId == boardId && bm.UserId == userId);
        }

        public async Task<bool> IsUserBoardOwnerAsync(Guid boardId, Guid userId)
        {
            var board = await _context.Boards
                .FirstOrDefaultAsync(b => b.Id == boardId);

            return board?.OwnerId == userId;
        }

        public async Task<BoardMember> AddBoardMemberAsync(Guid boardId, Guid userId, BoardMemberRole role)
        {
            var existingMember = await _context.BoardMembers
                .FirstOrDefaultAsync(bm => bm.BoardId == boardId && bm.UserId == userId);

            if (existingMember != null)
                throw new InvalidOperationException("User is already a member of this board");

            var boardMember = new BoardMember
            {
                Id = Guid.NewGuid(),
                BoardId = boardId,
                UserId = userId,
                Role = role,
                JoinedAt = DateTime.UtcNow
            };

            _context.BoardMembers.Add(boardMember);
            await _context.SaveChangesAsync();

            return boardMember;
        }

        public async Task RemoveBoardMemberAsync(Guid boardId, Guid userId)
        {
            var member = await _context.BoardMembers
                .FirstOrDefaultAsync(bm => bm.BoardId == boardId && bm.UserId == userId);

            if (member != null)
            {
                _context.BoardMembers.Remove(member);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateBoardMemberRoleAsync(Guid boardId, Guid userId, BoardMemberRole role)
        {
            var member = await _context.BoardMembers
                .FirstOrDefaultAsync(bm => bm.BoardId == boardId && bm.UserId == userId);

            if (member != null)
            {
                member.Role = role;
                await _context.SaveChangesAsync();
            }
        }
    }
}
