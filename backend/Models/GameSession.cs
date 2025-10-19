using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gameProject.Models
{
    public enum GameStatus
    {
        NotStarted,
        Active,
        Ended
    }

    [Table("GameSessions")]
    public class GameSession
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("room_id")]
        public int RoomId { get; set; }

        [Required]
        [Column("quiz_id")]
        public int QuizId { get; set; }

        [Required]
        [Column("status")]
        public GameStatus Status { get; set; } = GameStatus.NotStarted;

        [Column("current_question_number")]
        public int CurrentQuestionNumber { get; set; } = 0;

        [Column("current_question_end_time")]
        public DateTime? CurrentQuestionEndTime { get; set; }

        [Column("started_at")]
        public DateTime? StartedAt { get; set; }

        [Column("ended_at")]
        public DateTime? EndedAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("RoomId")]
        public Room? Room { get; set; }

        [ForeignKey("QuizId")]
        public Quiz? Quiz { get; set; }

        public GameRecord? GameRecord { get; set; }
        public ICollection<PlayerAnswer> PlayerAnswers { get; set; } = new List<PlayerAnswer>();
        public ICollection<Leaderboard> Leaderboards { get; set; } = new List<Leaderboard>();
    }
}