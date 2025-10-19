using Microsoft.EntityFrameworkCore;
using gameProject.Models;

namespace gameProject.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        } 

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Choice> Choices { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<GameSession> GameSessions { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<PlayerAnswer> PlayerAnswers { get; set; }
        public DbSet<Leaderboard> Leaderboards { get; set; }
        public DbSet<GameRecord> GameRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.AuthUuid)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_AuthUuid");

                entity.HasIndex(e => e.Username)
                    .HasDatabaseName("IX_Users_Username");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            // Quiz entity configuration
            modelBuilder.Entity<Quiz>(entity =>
            {
                entity.HasIndex(e => e.CreatorId)
                    .HasDatabaseName("IX_Quizzes_CreatorId");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(q => q.Creator)
                    .WithMany()
                    .HasForeignKey(q => q.CreatorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Question entity configuration
            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasIndex(e => e.QuizId)
                    .HasDatabaseName("IX_Questions_QuizId");

                entity.HasIndex(e => new { e.QuizId, e.QuestionOrder })
                    .HasDatabaseName("IX_Questions_QuizId_Order");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(q => q.Quiz)
                    .WithMany(qz => qz.Questions)
                    .HasForeignKey(q => q.QuizId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Choice entity configuration
            modelBuilder.Entity<Choice>(entity =>
            {
                entity.HasIndex(e => e.QuestionId)
                    .HasDatabaseName("IX_Choices_QuestionId");

                entity.HasOne(c => c.Question)
                    .WithMany(q => q.Choices)
                    .HasForeignKey(c => c.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Room entity configuration
            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasIndex(e => e.RoomCode)
                    .IsUnique()
                    .HasDatabaseName("IX_Rooms_RoomCode");

                entity.HasIndex(e => e.HostUserId)
                    .HasDatabaseName("IX_Rooms_HostUserId");

                entity.HasIndex(e => e.IsActive)
                    .HasDatabaseName("IX_Rooms_IsActive");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(r => r.Host)
                    .WithMany()
                    .HasForeignKey(r => r.HostUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // GameSession entity configuration
            modelBuilder.Entity<GameSession>(entity =>
            {
                entity.HasIndex(e => e.RoomId)
                    .HasDatabaseName("IX_GameSessions_RoomId");

                entity.HasIndex(e => e.QuizId)
                    .HasDatabaseName("IX_GameSessions_QuizId");

                entity.HasIndex(e => e.Status)
                    .HasDatabaseName("IX_GameSessions_Status");

                entity.Property(e => e.Status)
                    .HasConversion<string>();

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(gs => gs.Room)
                    .WithMany(r => r.GameSessions)
                    .HasForeignKey(gs => gs.RoomId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(gs => gs.Quiz)
                    .WithMany(q => q.GameSessions)
                    .HasForeignKey(gs => gs.QuizId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Player entity configuration
            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("IX_Players_UserId");

                entity.HasIndex(e => e.RoomId)
                    .HasDatabaseName("IX_Players_RoomId");

                entity.HasIndex(e => new { e.UserId, e.RoomId })
                    .IsUnique()
                    .HasDatabaseName("IX_Players_UserId_RoomId");

                entity.Property(e => e.JoinedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(p => p.User)
                    .WithMany()
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Room)
                    .WithMany(r => r.Players)
                    .HasForeignKey(p => p.RoomId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // PlayerAnswer entity configuration
            modelBuilder.Entity<PlayerAnswer>(entity =>
            {
                entity.HasIndex(e => e.PlayerId)
                    .HasDatabaseName("IX_PlayerAnswers_PlayerId");

                entity.HasIndex(e => e.GameSessionId)
                    .HasDatabaseName("IX_PlayerAnswers_GameSessionId");

                entity.HasIndex(e => e.QuestionId)
                    .HasDatabaseName("IX_PlayerAnswers_QuestionId");

                entity.HasIndex(e => new { e.PlayerId, e.QuestionId, e.GameSessionId })
                    .IsUnique()
                    .HasDatabaseName("IX_PlayerAnswers_Player_Question_Session");

                entity.Property(e => e.AnswerTime)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(pa => pa.Player)
                    .WithMany(p => p.PlayerAnswers)
                    .HasForeignKey(pa => pa.PlayerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(pa => pa.GameSession)
                    .WithMany(gs => gs.PlayerAnswers)
                    .HasForeignKey(pa => pa.GameSessionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pa => pa.Question)
                    .WithMany()
                    .HasForeignKey(pa => pa.QuestionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(pa => pa.Choice)
                    .WithMany(c => c.PlayerAnswers)
                    .HasForeignKey(pa => pa.ChoiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Leaderboard entity configuration
            modelBuilder.Entity<Leaderboard>(entity =>
            {
                entity.HasIndex(e => e.GameSessionId)
                    .HasDatabaseName("IX_Leaderboards_GameSessionId");

                entity.HasIndex(e => e.PlayerId)
                    .HasDatabaseName("IX_Leaderboards_PlayerId");

                entity.HasIndex(e => new { e.GameSessionId, e.PlayerId })
                    .IsUnique()
                    .HasDatabaseName("IX_Leaderboards_Session_Player");

                entity.HasIndex(e => new { e.GameSessionId, e.Score })
                    .HasDatabaseName("IX_Leaderboards_Session_Score");

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(l => l.GameSession)
                    .WithMany(gs => gs.Leaderboards)
                    .HasForeignKey(l => l.GameSessionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(l => l.Player)
                    .WithMany(p => p.Leaderboards)
                    .HasForeignKey(l => l.PlayerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // GameRecord entity configuration
            modelBuilder.Entity<GameRecord>(entity =>
            {
                entity.HasIndex(e => e.GameSessionId)
                    .IsUnique()
                    .HasDatabaseName("IX_GameRecords_GameSessionId");

                entity.HasIndex(e => e.QuizId)
                    .HasDatabaseName("IX_GameRecords_QuizId");

                entity.HasIndex(e => e.CreatedAt)
                    .HasDatabaseName("IX_GameRecords_CreatedAt");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(gr => gr.GameSession)
                    .WithOne(gs => gs.GameRecord)
                    .HasForeignKey<GameRecord>(gr => gr.GameSessionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(gr => gr.Quiz)
                    .WithMany()
                    .HasForeignKey(gr => gr.QuizId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(gr => gr.Winner)
                    .WithMany()
                    .HasForeignKey(gr => gr.WinnerPlayerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}       