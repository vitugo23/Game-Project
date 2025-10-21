using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gameProject.Models
{
    [Table("Leaderboards")]
    public class Leaderboard
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("game_session_id")]
        public int GameSessionId { get; set; }

        [Required]
        [Column("player_id")]
        public int PlayerId { get; set; }

        [Required]
        [Column("score")]
        public int Score { get; set; } = 0;

        [Column("rank")]
        public int Rank { get; set; }

        [Column("correct_answers")]
        public int CorrectAnswers { get; set; } = 0;

        [Column("total_answers")]
        public int TotalAnswers { get; set; } = 0;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("GameSessionId")]
        public GameSession? GameSession { get; set; }

        [ForeignKey("PlayerId")]
        public Player? Player { get; set; }
    }
}