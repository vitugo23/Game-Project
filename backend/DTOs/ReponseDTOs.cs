namespace GameProject.DTOs
{
    // User Response DTOs
    public class UserDto
    {
        public int Id { get; set; }
        public string AuthUuid { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // Quiz Response DTOs
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

    // Question Response DTOs
    public class QuestionDto
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public int QuestionOrder { get; set; }
        public int TimeLimit { get; set; }
        public List<ChoiceDto> Choices { get; set; } = new();
    }

    // Choice Response DTOs
    public class ChoiceDto
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string ChoiceText { get; set; } = string.Empty;
        public int ChoiceOrder { get; set; }
        // Note: IsCorrect is intentionally not included in the response for security
    }

    public class ChoiceWithCorrectDto : ChoiceDto
    {
        public bool IsCorrect { get; set; }
    }

    // Room Response DTOs
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

    // Player Response DTOs
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

    // GameSession Response DTOs
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

    // Leaderboard Response DTOs
    public class LeaderboardEntryDto
    {
        public int PlayerId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int Score { get; set; }
        public int Rank { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalAnswers { get; set; }
    }

    // GameRecord Response DTOs
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

    public class GameRecordDetailDto : GameRecordDto
    {
        public Dictionary<string, int> FinalLeaderboard { get; set; } = new();
    }

    // PlayerAnswer Response DTOs
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