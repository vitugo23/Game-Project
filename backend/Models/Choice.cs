using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gameProject.Models
{
    [Table("Choices")]
    public class Choice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("question_id")]
        public int QuestionId { get; set; }

        [Required]
        [Column("choice_text")]
        [MaxLength(500)]
        public string ChoiceText { get; set; } = string.Empty;

        [Required]
        [Column("is_correct")]
        public bool IsCorrect { get; set; } = false;

        [Column("choice_order")]
        public int ChoiceOrder { get; set; }

        [ForeignKey("QuestionId")]
        public Question? Question { get; set; }
        public ICollection<PlayerAnswer> PlayerAnswers { get; set; } = new List<PlayerAnswer>();
    }
}
