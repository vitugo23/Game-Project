using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;
using System.Security;

namespace gameProject.Models
{
    [Table("Rooms")]
    public class Room
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("room_code")]
        [MaxLength(10)]
        public string RoomCode { get; set; } = string.Empty;

        [Required]
        [Column("host_user_id")]
        public int HostUserId { get; set; }

        [Column("max_players")]
        public int MaxPlayers { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //Navigation Properties
        [ForeignKey("HostUserId")]
        public User? Host { get; set; }

        public ICollection<Player> Players { get; set; } = new List<Player>();
        public ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();

    }
}