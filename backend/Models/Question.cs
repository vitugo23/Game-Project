using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace gameProject.Models
{
    [Table("questions")]
    public class Question
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("quiz_id")]
        public int QuizId { get; set; }

        [Required]
        [Column("question_text")]
        [MaxLength(1000)]
        public string QuestionText { get; set; } = string.Empty;

        [Column("question_order")]
        public int QuestionOrder { get; set; }

        [Column("time_limit")]
        public int TimeLimit { get; set; } = 30;

        [Column("created_at")]
        public DataType CreatedAt { get; set; } = DateTime.UtcNow;

        //Navigation Properties
        [ForeignKey("quiz_id")]
        public Quiz? Quiz { get; set; }

        public ICollection<Choice> Choices { get; set; } = new List<Choice>();

    }
}