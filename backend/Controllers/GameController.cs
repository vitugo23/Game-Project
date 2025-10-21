using Microsoft.AspNetCore.Mvc;
using gameProject.DTOs;
using gameProject.Extensions;
using gameProject.Models;
using gameProject.Repositories.Interfaces;
using gameProject.Services;

namespace gameProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly IGameSessionRepository _gameSessionRepository;
        private readonly IPlayerAnswerRepository _playerAnswerRepository;
        private readonly IGameRecordRepository _gameRecordRepository;
        private readonly IQuizRepository _quizRepository;
        private readonly GameStatsService _gameStatsService;
        private readonly ILogger<GameController> _logger;

        public GameController(
            IGameSessionRepository gameSessionRepository,
            IPlayerAnswerRepository playerAnswerRepository,
            IGameRecordRepository gameRecordRepository,
            IQuizRepository quizRepository,
            GameStatsService gameStatsService,
            ILogger<GameController> logger)
        {
            _gameSessionRepository = gameSessionRepository;
            _playerAnswerRepository = playerAnswerRepository;
            _gameRecordRepository = gameRecordRepository;
            _quizRepository = quizRepository;
            _gameStatsService = gameStatsService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<GameSessionDetailDto>>> GetGameSession(int id)
        {
            try
            {
                var gameSession = await _gameSessionRepository.GetByIdWithDetailsAsync(id);
                if (gameSession == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Message = $"Game session with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<GameSessionDetailDto>
                {
                    Success = true,
                    Message = "Game session retrieved successfully",
                    Data = gameSession.ToDetailDto()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving game session with ID {GameSessionId}", id);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while retrieving the game session",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<GameSessionDto>>> CreateGameSession([FromBody] CreateGameSessionDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Invalid game session data",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                // Verify quiz exists
                var quiz = await _quizRepository.GetByIdWithQuestionsAsync(createDto.QuizId);
                if (quiz == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Message = $"Quiz with ID {createDto.QuizId} not found"
                    });
                }

                if (quiz.Questions == null || !quiz.Questions.Any())
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Quiz must have at least one question"
                    });
                }

                var gameSession = new GameSession
                {
                    RoomId = createDto.RoomId,
                    QuizId = createDto.QuizId,
                    Status = GameStatus.NotStarted,
                    CurrentQuestionNumber = 0
                };

                var createdSession = await _gameSessionRepository.CreateAsync(gameSession);

                return CreatedAtAction(
                    nameof(GetGameSession),
                    new { id = createdSession.Id },
                    new ApiResponse<GameSessionDto>
                    {
                        Success = true,
                        Message = "Game session created successfully",
                        Data = createdSession.ToDto()
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating game session");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while creating the game session",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPost("{id}/start")]
        public async Task<ActionResult<ApiResponse<GameSessionDto>>> StartGame(int id)
        {
            try
            {
                var gameSession = await _gameSessionRepository.GetByIdWithDetailsAsync(id);
                if (gameSession == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Message = $"Game session with ID {id} not found"
                    });
                }

                if (gameSession.Status != GameStatus.NotStarted)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Game has already been started"
                    });
                }

                gameSession.Status = GameStatus.Active;
                gameSession.StartedAt = DateTime.UtcNow;
                gameSession.CurrentQuestionNumber = 1;

                // Set end time for first question
                var firstQuestion = gameSession.Quiz?.Questions?.OrderBy(q => q.QuestionOrder).FirstOrDefault();
                if (firstQuestion != null)
                {
                    gameSession.CurrentQuestionEndTime = DateTime.UtcNow.AddSeconds(firstQuestion.TimeLimit);
                }

                var updatedSession = await _gameSessionRepository.UpdateAsync(gameSession);

                return Ok(new ApiResponse<GameSessionDto>
                {
                    Success = true,
                    Message = "Game started successfully",
                    Data = updatedSession!.ToDto()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting game session {GameSessionId}", id);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while starting the game",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPost("answer")]
        public async Task<ActionResult<ApiResponse<PlayerAnswerDto>>> SubmitAnswer([FromBody] SubmitAnswerDto submitDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Invalid answer data",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                // Check if answer already exists
                var existingAnswer = await _playerAnswerRepository.GetByPlayerQuestionAndSessionAsync(
                    submitDto.PlayerId, submitDto.QuestionId, submitDto.GameSessionId);

                if (existingAnswer != null)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Answer already submitted for this question"
                    });
                }

                // Get the choice to check if it's correct
                var choice = await _gameSessionRepository.GetByIdWithDetailsAsync(submitDto.GameSessionId);
                var question = choice?.Quiz?.Questions?.FirstOrDefault(q => q.Id == submitDto.QuestionId);
                var selectedChoice = question?.Choices?.FirstOrDefault(c => c.Id == submitDto.ChoiceId);

                if (selectedChoice == null)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Invalid choice selected"
                    });
                }

                bool isCorrect = selectedChoice.IsCorrect;
                int pointsEarned = _gameStatsService.CalculatePointsForAnswer(
                    isCorrect, submitDto.TimeTakenSeconds, question!.TimeLimit);

                var playerAnswer = new PlayerAnswer
                {
                    PlayerId = submitDto.PlayerId,
                    GameSessionId = submitDto.GameSessionId,
                    QuestionId = submitDto.QuestionId,
                    ChoiceId = submitDto.ChoiceId,
                    TimeTakenSeconds = submitDto.TimeTakenSeconds,
                    IsCorrect = isCorrect,
                    PointsEarned = pointsEarned
                };

                var createdAnswer = await _playerAnswerRepository.CreateAsync(playerAnswer);

                // Update leaderboard
                await _gameStatsService.UpdatePlayerLeaderboardAsync(
                    submitDto.PlayerId, submitDto.GameSessionId, pointsEarned, isCorrect);

                return Ok(new ApiResponse<PlayerAnswerDto>
                {
                    Success = true,
                    Message = "Answer submitted successfully",
                    Data = createdAnswer.ToDto()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting answer");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while submitting the answer",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("{id}/leaderboard")]
        public async Task<ActionResult<ApiResponse<Dictionary<string, int>>>> GetLeaderboard(int id)
        {
            try
            {
                var leaderboard = await _gameStatsService.GetCurrentLeaderboardAsync(id);

                return Ok(new ApiResponse<Dictionary<string, int>>
                {
                    Success = true,
                    Message = "Leaderboard retrieved successfully",
                    Data = leaderboard
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving leaderboard for game session {GameSessionId}", id);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while retrieving the leaderboard",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPost("{id}/end")]
        public async Task<ActionResult<ApiResponse<GameRecordDto>>> EndGame(int id)
        {
            try
            {
                var gameSession = await _gameSessionRepository.GetByIdWithDetailsAsync(id);
                if (gameSession == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Message = $"Game session with ID {id} not found"
                    });
                }

                if (gameSession.Status == GameStatus.Ended)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Game has already ended"
                    });
                }

                gameSession.Status = GameStatus.Ended;
                gameSession.EndedAt = DateTime.UtcNow;
                await _gameSessionRepository.UpdateAsync(gameSession);

                // Get final leaderboard
                var leaderboard = await _gameStatsService.GetCurrentLeaderboardAsync(id);
                var winner = leaderboard.OrderByDescending(l => l.Value).FirstOrDefault();

                // Calculate game duration
                var duration = gameSession.EndedAt.Value - (gameSession.StartedAt ?? gameSession.CreatedAt);

                // Create game record
                var gameRecord = new GameRecord
                {
                    GameSessionId = id,
                    QuizId = gameSession.QuizId,
                    TotalPlayers = gameSession.Room?.Players?.Count ?? 0,
                    GameDurationSeconds = (int)duration.TotalSeconds
                };

                var createdRecord = await _gameRecordRepository.CreateGameRecordAsync(gameRecord);

                return Ok(new ApiResponse<GameRecordDto>
                {
                    Success = true,
                    Message = "Game ended successfully",
                    Data = createdRecord.ToDto()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending game session {GameSessionId}", id);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while ending the game",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}