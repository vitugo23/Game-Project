using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gameProject.Models
{
    [Table("Players")]
    public class Player
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("room_id")]
        public int RoomId { get; set; }

        [Column("is_ready")]
        public bool IsReady { get; set; } = false;

        [Column("is_connected")]
        public bool IsConnected { get; set; } = true;

        [Column("joined_at")]
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("RoomId")]
        public Room? Room { get; set; }

        public ICollection<PlayerAnswer> PlayerAnswers { get; set; } = new List<PlayerAnswer>();
        public ICollection<Leaderboard> Leaderboards { get; set; } = new List<Leaderboard>();
    }
}