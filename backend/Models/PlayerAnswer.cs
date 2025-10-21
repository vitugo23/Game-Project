using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gameProject.Models
{
    [Table("PlayerAnswers")]
    public class PlayerAnswer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("player_id")]
        public int PlayerId { get; set; }

        [Required]
        [Column("game_session_id")]
        public int GameSessionId { get; set; }

        [Required]
        [Column("question_id")]
        public int QuestionId { get; set; }

        [Required]
        [Column("choice_id")]
        public int ChoiceId { get; set; }

        [Column("answer_time")]
        public DateTime AnswerTime { get; set; } = DateTime.UtcNow;

        [Column("time_taken_seconds")]
        public double TimeTakenSeconds { get; set; }

        [Column("is_correct")]
        public bool IsCorrect { get; set; }

        [Column("points_earned")]
        public int PointsEarned { get; set; } = 0;

        // Navigation properties
        [ForeignKey("PlayerId")]
        public Player? Player { get; set; }

        [ForeignKey("GameSessionId")]
        public GameSession? GameSession { get; set; }

        [ForeignKey("QuestionId")]
        public Question? Question { get; set; }

        [ForeignKey("ChoiceId")]
        public Choice? Choice { get; set; }
    }
}