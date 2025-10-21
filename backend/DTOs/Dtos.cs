using System.ComponentModel.DataAnnotations;

namespace gameProject.DTOs
{
    // User DTOs
    public class UserDto
    {
        public int Id { get; set; }
        public string AuthUuid { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateUserDto
    {
        [Required]
        public string AuthUuid { get; set; } = string.Empty;
        
        [Required]
        public string Username { get; set; } = string.Empty;
    }

    public class UpdateUserDto
    {
        public string? Username { get; set; }
    }

    // Quiz DTOs
    public class QuizDto
    {
        public int Id { get; set; }
        public string QuizName { get; set; } = string.Empty;
        public int CreatorId { get; set; }
        public string? CreatorUsername { get; set; }
        public int QuestionCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class QuizDetailDto
    {
        public int Id { get; set; }
        public string QuizName { get; set; } = string.Empty;
        public int CreatorId { get; set; }
        public string? CreatorUsername { get; set; }
        public List<QuestionDto> Questions { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    public class CreateQuizDto
    {
        [Required]
        public string QuizName { get; set; } = string.Empty;
        
        [Required]
        public int CreatorId { get; set; }
        
        public List<CreateQuestionDto> Questions { get; set; } = new();
    }

    public class UpdateQuizDto
    {
        public string? QuizName { get; set; }
    }

    // Question DTOs
    public class QuestionDto
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public int QuestionOrder { get; set; }
        public int TimeLimit { get; set; }
        public List<ChoiceDto> Choices { get; set; } = new();
    }

    public class CreateQuestionDto
    {
        [Required]
        public string QuestionText { get; set; } = string.Empty;
        public int QuestionOrder { get; set; }
        public int TimeLimit { get; set; } = 30;
        public List<CreateChoiceDto> Choices { get; set; } = new();
    }

    public class UpdateQuestionDto
    {
        public string? QuestionText { get; set; }
        public int? QuestionOrder { get; set; }
        public int? TimeLimit { get; set; }
    }

    // Choice DTOs
    public class ChoiceDto
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string ChoiceText { get; set; } = string.Empty;
        public int ChoiceOrder { get; set; }
    }

    public class ChoiceWithCorrectDto : ChoiceDto
    {
        public bool IsCorrect { get; set; }
    }

    public class CreateChoiceDto
    {
        [Required]
        public string ChoiceText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int ChoiceOrder { get; set; }
    }

    // Room DTOs
    public class RoomDto
    {
        public int Id { get; set; }
        public string RoomCode { get; set; } = string.Empty;
        public int HostUserId { get; set; }
        public string? HostUsername { get; set; }
        public int MaxPlayers { get; set; }
        public int CurrentPlayers { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RoomDetailDto : RoomDto
    {
        public List<PlayerDto> Players { get; set; } = new();
    }

    public class CreateRoomDto
    {
        [Required]
        public int HostUserId { get; set; }
        public int MaxPlayers { get; set; } = 10;
    }

    public class UpdateRoomDto
    {
        public int? MaxPlayers { get; set; }
        public bool? IsActive { get; set; }
    }

    public class JoinRoomDto
    {
        [Required]
        public string RoomCode { get; set; } = string.Empty;
        
        [Required]
        public int UserId { get; set; }
    }

    // Player DTOs
    public class PlayerDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int RoomId { get; set; }
        public bool IsReady { get; set; }
        public bool IsConnected { get; set; }
        public DateTime JoinedAt { get; set; }
    }

    // GameSession DTOs
    public class GameSessionDto
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int QuizId { get; set; }
        public string QuizName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int CurrentQuestionNumber { get; set; }
        public DateTime? CurrentQuestionEndTime { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
    }

    public class GameSessionDetailDto : GameSessionDto
    {
        public List<QuestionDto> Questions { get; set; } = new();
        public List<LeaderboardEntryDto> Leaderboard { get; set; } = new();
    }

    public class CreateGameSessionDto
    {
        [Required]
        public int RoomId { get; set; }
        
        [Required]
        public int QuizId { get; set; }
    }

    // Leaderboard DTOs
    public class LeaderboardEntryDto
    {
        public int PlayerId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int Score { get; set; }
        public int Rank { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalAnswers { get; set; }
    }

    // PlayerAnswer DTOs
    public class PlayerAnswerDto
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public string PlayerUsername { get; set; } = string.Empty;
        public int QuestionId { get; set; }
        public int ChoiceId { get; set; }
        public bool IsCorrect { get; set; }
        public int PointsEarned { get; set; }
        public double TimeTakenSeconds { get; set; }
        public DateTime AnswerTime { get; set; }
    }

    public class SubmitAnswerDto
    {
        [Required]
        public int PlayerId { get; set; }
        
        [Required]
        public int GameSessionId { get; set; }
        
        [Required]
        public int QuestionId { get; set; }
        
        [Required]
        public int ChoiceId { get; set; }
        
        [Required]
        public double TimeTakenSeconds { get; set; }
    }

    // GameRecord DTOs
    public class GameRecordDto
    {
        public int Id { get; set; }
        public int GameSessionId { get; set; }
        public int QuizId { get; set; }
        public string QuizName { get; set; } = string.Empty;
        public int? WinnerPlayerId { get; set; }
        public string? WinnerUsername { get; set; }
        public int TotalPlayers { get; set; }
        public int GameDurationSeconds { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // API Response Wrappers
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    public class ErrorResponse
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
    }
}