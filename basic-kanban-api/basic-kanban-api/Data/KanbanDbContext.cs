using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using basic_kanban_api.Models;

namespace basic_kanban_api.Data
{
    public class KanbanDbContext : IdentityDbContext<User, Role, Guid>
    {
        public DbSet<Board> Boards { get; set; }
        public DbSet<CardList> CardLists { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<BoardMember> BoardMembers { get; set; }

        public KanbanDbContext(DbContextOptions<KanbanDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração do modelo Board
            modelBuilder.Entity<Board>()
                .HasOne(b => b.Owner)
                .WithMany(u => u.CreatedBoards)
                .HasForeignKey(b => b.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Board>()
                .HasMany(b => b.CardLists)
                .WithOne(cl => cl.Board)
                .HasForeignKey(cl => cl.BoardId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Board>()
                .HasMany(b => b.Members)
                .WithOne(bm => bm.Board)
                .HasForeignKey(bm => bm.BoardId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuração do modelo CardList
            modelBuilder.Entity<CardList>()
                .HasMany(cl => cl.Cards)
                .WithOne(c => c.CardList)
                .HasForeignKey(c => c.CardListId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuração do modelo Card
            modelBuilder.Entity<Card>()
                .HasOne(c => c.AssignedToUser)
                .WithMany(u => u.AssignedCards)
                .HasForeignKey(c => c.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Configuração do modelo BoardMember
            modelBuilder.Entity<BoardMember>()
                .HasOne(bm => bm.User)
                .WithMany(u => u.BoardMemberships)
                .HasForeignKey(bm => bm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BoardMember>()
                .HasIndex(bm => new { bm.BoardId, bm.UserId })
                .IsUnique();

            // Índices para melhor performance
            modelBuilder.Entity<Board>()
                .HasIndex(b => b.OwnerId);

            modelBuilder.Entity<CardList>()
                .HasIndex(cl => cl.BoardId);

            modelBuilder.Entity<Card>()
                .HasIndex(c => c.CardListId);

            modelBuilder.Entity<Card>()
                .HasIndex(c => c.AssignedToUserId);
        }
    }
}
