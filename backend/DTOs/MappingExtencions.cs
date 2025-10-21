using gameProject.Models;
using gameProject.DTOs;

namespace gameProject.Extensions
{
    public static class MappingExtensions
    {
        // User mappings
        public static UserDto ToDto(this User user)
        {
            return new UserDto
            {
                Id = user.Id,
                AuthUuid = user.AuthUuid,
                Username = user.Username,
                CreatedAt = user.CreatedAt
            };
        }

        // Quiz mappings
        public static QuizDto ToDto(this Quiz quiz)
        {
            return new QuizDto
            {
                Id = quiz.Id,
                QuizName = quiz.QuizName,
                CreatorId = quiz.CreatorId,
                CreatorUsername = quiz.Creator?.Username,
                QuestionCount = quiz.Questions?.Count ?? 0,
                CreatedAt = quiz.CreatedAt
            };
        }

        public static QuizDetailDto ToDetailDto(this Quiz quiz)
        {
            return new QuizDetailDto
            {
                Id = quiz.Id,
                QuizName = quiz.QuizName,
                CreatorId = quiz.CreatorId,
                CreatorUsername = quiz.Creator?.Username,
                Questions = quiz.Questions?.Select(q => q.ToDto()).ToList() ?? new(),
                CreatedAt = quiz.CreatedAt
            };
        }

        // Question mappings
        public static QuestionDto ToDto(this Question question)
        {
            return new QuestionDto
            {
                Id = question.Id,
                QuizId = question.QuizId,
                QuestionText = question.QuestionText,
                QuestionOrder = question.QuestionOrder,
                TimeLimit = question.TimeLimit,
                Choices = question.Choices?.Select(c => c.ToDto()).ToList() ?? new()
            };
        }

        // Choice mappings
        public static ChoiceDto ToDto(this Choice choice)
        {
            return new ChoiceDto
            {
                Id = choice.Id,
                QuestionId = choice.QuestionId,
                ChoiceText = choice.ChoiceText,
                ChoiceOrder = choice.ChoiceOrder
            };
        }

        public static ChoiceWithCorrectDto ToDtoWithCorrect(this Choice choice)
        {
            return new ChoiceWithCorrectDto
            {
                Id = choice.Id,
                QuestionId = choice.QuestionId,
                ChoiceText = choice.ChoiceText,
                ChoiceOrder = choice.ChoiceOrder,
                IsCorrect = choice.IsCorrect
            };
        }

        // Room mappings
        public static RoomDto ToDto(this Room room)
        {
            return new RoomDto
            {
                Id = room.Id,
                RoomCode = room.RoomCode,
                HostUserId = room.HostUserId,
                HostUsername = room.Host?.Username,
                MaxPlayers = room.MaxPlayers,
                CurrentPlayers = room.Players?.Count ?? 0,
                IsActive = room.IsActive,
                CreatedAt = room.CreatedAt
            };
        }

        public static RoomDetailDto ToDetailDto(this Room room)
        {
            return new RoomDetailDto
            {
                Id = room.Id,
                RoomCode = room.RoomCode,
                HostUserId = room.HostUserId,
                HostUsername = room.Host?.Username,
                MaxPlayers = room.MaxPlayers,
                CurrentPlayers = room.Players?.Count ?? 0,
                IsActive = room.IsActive,
                CreatedAt = room.CreatedAt,
                Players = room.Players?.Select(p => p.ToDto()).ToList() ?? new()
            };
        }

        // Player mappings
        public static PlayerDto ToDto(this Player player)
        {
            return new PlayerDto
            {
                Id = player.Id,
                UserId = player.UserId,
                Username = player.User?.Username ?? string.Empty,
                RoomId = player.RoomId,
                IsReady = player.IsReady,
                IsConnected = player.IsConnected,
                JoinedAt = player.JoinedAt
            };
        }

        // GameSession mappings
        public static GameSessionDto ToDto(this GameSession gameSession)
        {
            return new GameSessionDto
            {
                Id = gameSession.Id,
                RoomId = gameSession.RoomId,
                QuizId = gameSession.QuizId,
                QuizName = gameSession.Quiz?.QuizName ?? string.Empty,
                Status = gameSession.Status.ToString(),
                CurrentQuestionNumber = gameSession.CurrentQuestionNumber,
                CurrentQuestionEndTime = gameSession.CurrentQuestionEndTime,
                StartedAt = gameSession.StartedAt,
                EndedAt = gameSession.EndedAt
            };
        }

        public static GameSessionDetailDto ToDetailDto(this GameSession gameSession)
        {
            return new GameSessionDetailDto
            {
                Id = gameSession.Id,
                RoomId = gameSession.RoomId,
                QuizId = gameSession.QuizId,
                QuizName = gameSession.Quiz?.QuizName ?? string.Empty,
                Status = gameSession.Status.ToString(),
                CurrentQuestionNumber = gameSession.CurrentQuestionNumber,
                CurrentQuestionEndTime = gameSession.CurrentQuestionEndTime,
                StartedAt = gameSession.StartedAt,
                EndedAt = gameSession.EndedAt,
                Questions = gameSession.Quiz?.Questions?.Select(q => q.ToDto()).ToList() ?? new(),
                Leaderboard = gameSession.Leaderboards?.Select(l => l.ToDto()).ToList() ?? new()
            };
        }

        // Leaderboard mappings
        public static LeaderboardEntryDto ToDto(this Leaderboard leaderboard)
        {
            return new LeaderboardEntryDto
            {
                PlayerId = leaderboard.PlayerId,
                Username = leaderboard.Player?.User?.Username ?? string.Empty,
                Score = leaderboard.Score,
                Rank = leaderboard.Rank,
                CorrectAnswers = leaderboard.CorrectAnswers,
                TotalAnswers = leaderboard.TotalAnswers
            };
        }

        // GameRecord mappings
        public static GameRecordDto ToDto(this GameRecord gameRecord)
        {
            return new GameRecordDto
            {
                Id = gameRecord.Id,
                GameSessionId = gameRecord.GameSessionId,
                QuizId = gameRecord.QuizId,
                QuizName = gameRecord.Quiz?.QuizName ?? string.Empty,
                WinnerPlayerId = gameRecord.WinnerPlayerId,
                WinnerUsername = gameRecord.Winner?.User?.Username,
                TotalPlayers = gameRecord.TotalPlayers,
                GameDurationSeconds = gameRecord.GameDurationSeconds,
                CreatedAt = gameRecord.CreatedAt
            };
        }

        // PlayerAnswer mappings
        public static PlayerAnswerDto ToDto(this PlayerAnswer playerAnswer)
        {
            return new PlayerAnswerDto
            {
                Id = playerAnswer.Id,
                PlayerId = playerAnswer.PlayerId,
                PlayerUsername = playerAnswer.Player?.User?.Username ?? string.Empty,
                QuestionId = playerAnswer.QuestionId,
                ChoiceId = playerAnswer.ChoiceId,
                IsCorrect = playerAnswer.IsCorrect,
                PointsEarned = playerAnswer.PointsEarned,
                TimeTakenSeconds = playerAnswer.TimeTakenSeconds,
                AnswerTime = playerAnswer.AnswerTime
            };
        }
    }
}