using Microsoft.AspNetCore.SignalR;
using gameProject.Models;
using gameProject.Repositories.Interfaces;
using gameProject.Services;
using gameProject.DTOs;

namespace gameProject.Hubs
{
    public class GameHub : Hub
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly IGameSessionRepository _gameSessionRepository;
        private readonly IPlayerAnswerRepository _playerAnswerRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly GameStatsService _gameStatsService;
        private readonly ILogger<GameHub> _logger;

        public GameHub(
            IRoomRepository roomRepository,
            IPlayerRepository playerRepository,
            IGameSessionRepository gameSessionRepository,
            IPlayerAnswerRepository playerAnswerRepository,
            IQuestionRepository questionRepository,
            GameStatsService gameStatsService,
            ILogger<GameHub> logger)
        {
            _roomRepository = roomRepository;
            _playerRepository = playerRepository;
            _gameSessionRepository = gameSessionRepository;
            _playerAnswerRepository = playerAnswerRepository;
            _questionRepository = questionRepository;
            _gameStatsService = gameStatsService;
            _logger = logger;
        }

        // Connection Management
        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        // Room Management
        public async Task JoinRoom(string roomCode, int userId, string username)
        {
            try
            {
                var room = await _roomRepository.GetByRoomCodeAsync(roomCode);
                if (room == null)
                {
                    await Clients.Caller.SendAsync("Error", "Room not found");
                    return;
                }

                // Add connection to room group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"room_{room.Id}");

                // Get or create player
                var player = await _playerRepository.GetByUserAndRoomAsync(userId, room.Id);
                if (player == null)
                {
                    player = new Player
                    {
                        UserId = userId,
                        RoomId = room.Id,
                        IsReady = false,
                        IsConnected = true
                    };
                    player = await _playerRepository.CreateAsync(player);
                }
                else
                {
                    player.IsConnected = true;
                    await _playerRepository.UpdateAsync(player);
                }

                // Notify all clients in room
                await Clients.Group($"room_{room.Id}").SendAsync("PlayerJoined", new
                {
                    PlayerId = player.Id,
                    UserId = userId,
                    Username = username,
                    IsReady = player.IsReady,
                    Timestamp = DateTime.UtcNow
                });

                // Send current room state to the joining player
                var players = await _playerRepository.GetByRoomIdAsync(room.Id);
                await Clients.Caller.SendAsync("RoomState", new
                {
                    RoomCode = room.RoomCode,
                    HostUserId = room.HostUserId,
                    Players = players.Select(p => new
                    {
                        p.Id,
                        p.UserId,
                        Username = p.User?.Username,
                        p.IsReady,
                        p.IsConnected
                    }),
                    MaxPlayers = room.MaxPlayers,
                    IsActive = room.IsActive
                });

                _logger.LogInformation("User {UserId} joined room {RoomCode}", userId, roomCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining room {RoomCode}", roomCode);
                await Clients.Caller.SendAsync("Error", "Failed to join room");
            }
        }

        public async Task LeaveRoom(int roomId, int playerId)
        {
            try
            {
                var player = await _playerRepository.GetByIdAsync(playerId);
                if (player != null)
                {
                    player.IsConnected = false;
                    await _playerRepository.UpdateAsync(player);

                    await Clients.Group($"room_{roomId}").SendAsync("PlayerLeft", new
                    {
                        PlayerId = playerId,
                        UserId = player.UserId,
                        Username = player.User?.Username,
                        Timestamp = DateTime.UtcNow
                    });
                }

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"room_{roomId}");
                _logger.LogInformation("Player {PlayerId} left room {RoomId}", playerId, roomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving room {RoomId}", roomId);
            }
        }

        public async Task SetReady(int playerId, bool isReady)
        {
            try
            {
                var player = await _playerRepository.GetByIdAsync(playerId);
                if (player == null)
                {
                    await Clients.Caller.SendAsync("Error", "Player not found");
                    return;
                }

                player.IsReady = isReady;
                await _playerRepository.UpdateAsync(player);

                await Clients.Group($"room_{player.RoomId}").SendAsync("PlayerReadyChanged", new
                {
                    PlayerId = playerId,
                    IsReady = isReady,
                    Username = player.User?.Username,
                    Timestamp = DateTime.UtcNow
                });

                _logger.LogInformation("Player {PlayerId} ready status: {IsReady}", playerId, isReady);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting ready status for player {PlayerId}", playerId);
                await Clients.Caller.SendAsync("Error", "Failed to update ready status");
            }
        }

        // Game Management
        public async Task StartGame(int gameSessionId)
        {
            try
            {
                var gameSession = await _gameSessionRepository.GetByIdWithDetailsAsync(gameSessionId);
                if (gameSession == null)
                {
                    await Clients.Caller.SendAsync("Error", "Game session not found");
                    return;
                }

                if (gameSession.Status != GameStatus.NotStarted)
                {
                    await Clients.Caller.SendAsync("Error", "Game already started");
                    return;
                }

                gameSession.Status = GameStatus.Active;
                gameSession.StartedAt = DateTime.UtcNow;
                gameSession.CurrentQuestionNumber = 1;

                var firstQuestion = gameSession.Quiz?.Questions?.OrderBy(q => q.QuestionOrder).FirstOrDefault();
                if (firstQuestion != null)
                {
                    gameSession.CurrentQuestionEndTime = DateTime.UtcNow.AddSeconds(firstQuestion.TimeLimit);
                }

                await _gameSessionRepository.UpdateAsync(gameSession);

                // Notify all players in the room
                await Clients.Group($"room_{gameSession.RoomId}").SendAsync("GameStarted", new
                {
                    GameSessionId = gameSessionId,
                    StartedAt = gameSession.StartedAt,
                    TotalQuestions = gameSession.Quiz?.Questions?.Count ?? 0,
                    Timestamp = DateTime.UtcNow
                });

                // Send first question
                if (firstQuestion != null)
                {
                    await SendQuestion(gameSession.RoomId, firstQuestion, gameSession.CurrentQuestionNumber);
                }

                _logger.LogInformation("Game {GameSessionId} started", gameSessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting game {GameSessionId}", gameSessionId);
                await Clients.Caller.SendAsync("Error", "Failed to start game");
            }
        }

        public async Task SubmitAnswer(int playerId, int gameSessionId, int questionId, int choiceId, double timeTaken)
        {
            try
            {
                // Check if answer already submitted
                var existingAnswer = await _playerAnswerRepository.GetByPlayerQuestionAndSessionAsync(
                    playerId, questionId, gameSessionId);

                if (existingAnswer != null)
                {
                    await Clients.Caller.SendAsync("Error", "Answer already submitted");
                    return;
                }

                // Get game session and question details
                var gameSession = await _gameSessionRepository.GetByIdWithDetailsAsync(gameSessionId);
                var question = gameSession?.Quiz?.Questions?.FirstOrDefault(q => q.Id == questionId);
                var choice = question?.Choices?.FirstOrDefault(c => c.Id == choiceId);

                if (choice == null)
                {
                    await Clients.Caller.SendAsync("Error", "Invalid choice");
                    return;
                }

                bool isCorrect = choice.IsCorrect;
                int pointsEarned = _gameStatsService.CalculatePointsForAnswer(
                    isCorrect, timeTaken, question!.TimeLimit);

                var playerAnswer = new PlayerAnswer
                {
                    PlayerId = playerId,
                    GameSessionId = gameSessionId,
                    QuestionId = questionId,
                    ChoiceId = choiceId,
                    TimeTakenSeconds = timeTaken,
                    IsCorrect = isCorrect,
                    PointsEarned = pointsEarned
                };

                await _playerAnswerRepository.CreateAsync(playerAnswer);

                // Update leaderboard
                await _gameStatsService.UpdatePlayerLeaderboardAsync(
                    playerId, gameSessionId, pointsEarned, isCorrect);

                var player = await _playerRepository.GetByIdAsync(playerId);

                // Notify room that player submitted answer
                await Clients.Group($"room_{gameSession!.RoomId}").SendAsync("AnswerSubmitted", new
                {
                    PlayerId = playerId,
                    Username = player?.User?.Username,
                    QuestionId = questionId,
                    TimeTaken = timeTaken,
                    Timestamp = DateTime.UtcNow
                });

                // Send updated leaderboard
                await SendLeaderboard(gameSession.RoomId, gameSessionId);

                _logger.LogInformation("Player {PlayerId} submitted answer for question {QuestionId}", 
                    playerId, questionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting answer");
                await Clients.Caller.SendAsync("Error", "Failed to submit answer");
            }
        }

        public async Task NextQuestion(int gameSessionId)
        {
            try
            {
                var gameSession = await _gameSessionRepository.GetByIdWithDetailsAsync(gameSessionId);
                if (gameSession == null)
                {
                    await Clients.Caller.SendAsync("Error", "Game session not found");
                    return;
                }

                var questions = gameSession.Quiz?.Questions?.OrderBy(q => q.QuestionOrder).ToList();
                if (questions == null || !questions.Any())
                {
                    await Clients.Caller.SendAsync("Error", "No questions found");
                    return;
                }

                var currentQuestionNumber = gameSession.CurrentQuestionNumber;
                var currentQuestion = questions.ElementAtOrDefault(currentQuestionNumber - 1);

                // Reveal correct answer for current question
                if (currentQuestion != null)
                {
                    var correctChoice = currentQuestion.Choices?.FirstOrDefault(c => c.IsCorrect);
                    await Clients.Group($"room_{gameSession.RoomId}").SendAsync("QuestionEnded", new
                    {
                        QuestionId = currentQuestion.Id,
                        CorrectChoiceId = correctChoice?.Id,
                        CorrectChoiceText = correctChoice?.ChoiceText,
                        Timestamp = DateTime.UtcNow
                    });
                }

                // Check if there are more questions
                if (currentQuestionNumber >= questions.Count)
                {
                    // Game is over
                    await EndGame(gameSessionId);
                    return;
                }

                // Move to next question
                gameSession.CurrentQuestionNumber++;
                var nextQuestion = questions[gameSession.CurrentQuestionNumber - 1];
                gameSession.CurrentQuestionEndTime = DateTime.UtcNow.AddSeconds(nextQuestion.TimeLimit);

                await _gameSessionRepository.UpdateAsync(gameSession);

                // Send next question
                await SendQuestion(gameSession.RoomId, nextQuestion, gameSession.CurrentQuestionNumber);

                _logger.LogInformation("Game {GameSessionId} moved to question {QuestionNumber}", 
                    gameSessionId, gameSession.CurrentQuestionNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving to next question");
                await Clients.Caller.SendAsync("Error", "Failed to move to next question");
            }
        }

        public async Task EndGame(int gameSessionId)
        {
            try
            {
                var gameSession = await _gameSessionRepository.GetByIdWithDetailsAsync(gameSessionId);
                if (gameSession == null)
                {
                    await Clients.Caller.SendAsync("Error", "Game session not found");
                    return;
                }

                gameSession.Status = GameStatus.Ended;
                gameSession.EndedAt = DateTime.UtcNow;
                await _gameSessionRepository.UpdateAsync(gameSession);

                // Get final leaderboard
                var leaderboard = await _gameStatsService.GetCurrentLeaderboardAsync(gameSessionId);
                var stats = await _gameStatsService.GetGameStatisticsAsync(gameSessionId);

                // Notify all players
                await Clients.Group($"room_{gameSession.RoomId}").SendAsync("GameEnded", new
                {
                    GameSessionId = gameSessionId,
                    EndedAt = gameSession.EndedAt,
                    FinalLeaderboard = leaderboard,
                    Statistics = stats,
                    Timestamp = DateTime.UtcNow
                });

                _logger.LogInformation("Game {GameSessionId} ended", gameSessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending game {GameSessionId}", gameSessionId);
                await Clients.Caller.SendAsync("Error", "Failed to end game");
            }
        }

        // Helper Methods
        private async Task SendQuestion(int roomId, Question question, int questionNumber)
        {
            await Clients.Group($"room_{roomId}").SendAsync("QuestionStarted", new
            {
                QuestionNumber = questionNumber,
                QuestionId = question.Id,
                QuestionText = question.QuestionText,
                TimeLimit = question.TimeLimit,
                EndTime = DateTime.UtcNow.AddSeconds(question.TimeLimit),
                Choices = question.Choices?.OrderBy(c => c.ChoiceOrder).Select(c => new
                {
                    c.Id,
                    c.ChoiceText,
                    c.ChoiceOrder
                    // Note: IsCorrect is NOT sent to clients
                }),
                Timestamp = DateTime.UtcNow
            });
        }

        private async Task SendLeaderboard(int roomId, int gameSessionId)
        {
            var leaderboard = await _gameStatsService.GetCurrentLeaderboardAsync(gameSessionId);

            await Clients.Group($"room_{roomId}").SendAsync("LeaderboardUpdated", new
            {
                GameSessionId = gameSessionId,
                Leaderboard = leaderboard.Select((kvp, index) => new
                {
                    Rank = index + 1,
                    Username = kvp.Key,
                    Score = kvp.Value
                }),
                Timestamp = DateTime.UtcNow
            });
        }
    }
}