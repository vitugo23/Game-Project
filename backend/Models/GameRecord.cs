using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gameProject.Models
{
    [Table("Game Records")]
    public class GameRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("game_session_id")]
        public int GameSessionId { get; set; }

        [Required]
        [Column("quiz_id")]
        public int QuizId { get; set; }

        [Column("winner_player_id")]
        public int? WinnerPlayerId { get; set; }

        [Column("total_players")]
        public int TotalPlayers { get; set; }

        [Column("game_duration_seconds")]
        public int GameDurationSeconds { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("GameSessionId")]
        public GameSession? GameSession { get; set; }

        [ForeignKey("QuizId")]
        public Quiz? Quiz { get; set; }

        [ForeignKey("WinnerPlayerId")]
        public Player? Winner { get; set; }
    }
}