using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gameProject.Models
{
    [Table("Quizzes")]
    public class Quiz
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("quiz_name")]
        [MaxLength(200)]
        public string QuizName { get; set; } = string.Empty;

        [Required]
        [Column("creator_id")]
        public int CreatorId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("CreatorId")]
        public User? Creator { get; set; }

        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();

    }
}