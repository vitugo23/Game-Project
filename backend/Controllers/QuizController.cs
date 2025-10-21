using Microsoft.AspNetCore.Mvc;
using System.Linq;
using gameProject.DTOs;
using gameProject.Extensions;
using gameProject.Models;
using gameProject.Repositories.Interfaces;

namespace gameProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly IQuizRepository _quizRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly ILogger<QuizController> _logger;

        public QuizController(
            IQuizRepository quizRepository,
            IQuestionRepository questionRepository,
            ILogger<QuizController> logger)
        {
            _quizRepository = quizRepository;
            _questionRepository = questionRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<QuizDto>>>> GetAllQuizzes()
        {
            try
            {
                var quizzes = await _quizRepository.GetAllByAsync();
                var quizDtos = quizzes.Select(q => q.ToDto()).ToList();

                return Ok(new ApiResponse<List<QuizDto>>
                {
                    Success = true,
                    Message = "Quizzes retrieved successfully",
                    Data = quizDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all quizzes");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while retrieving quizzes",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<QuizDetailDto>>> GetQuizById(int id)
        {
            try
            {
                var quiz = await _quizRepository.GetByIdWithQuestionsAsync(id);
                if (quiz == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Message = $"Quiz with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<QuizDetailDto>
                {
                    Success = true,
                    Message = "Quiz retrieved successfully",
                    Data = quiz.ToDetailDto()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving quiz with ID {QuizId}", id);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while retrieving the quiz",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("creator/{creatorId}")]
        public async Task<ActionResult<ApiResponse<List<QuizDto>>>> GetQuizzesByCreator(int creatorId)
        {
            try
            {
                var quizzes = await _quizRepository.GetByCreatorIdAsync(creatorId);
                var quizDtos = await quizzes.Select(q => q.ToDto()).ToListAsync();

                return Ok(new ApiResponse<List<QuizDto>>
                {
                    Success = true,
                    Message = "Quizzes retrieved successfully",
                    Data = quizDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving quizzes for creator {CreatorId}", creatorId);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while retrieving quizzes",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<QuizDto>>> CreateQuiz([FromBody] CreateQuizDto createQuizDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Invalid quiz data",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                var quiz = new Quiz
                {
                    QuizName = createQuizDto.QuizName,
                    CreatorId = createQuizDto.CreatorId
                };

                var createdQuiz = await _quizRepository.CreateAsync(quiz);

                // Create questions if provided
                if (createQuizDto.Questions != null && createQuizDto.Questions.Any())
                {
                    foreach (var questionDto in createQuizDto.Questions)
                    {
                        var question = new Question
                        {
                            QuizId = createdQuiz.Id,
                            QuestionText = questionDto.QuestionText,
                            QuestionOrder = questionDto.QuestionOrder,
                            TimeLimit = questionDto.TimeLimit,
                            Choices = questionDto.Choices.Select(c => new Choice
                            {
                                ChoiceText = c.ChoiceText,
                                IsCorrect = c.IsCorrect,
                                ChoiceOrder = c.ChoiceOrder
                            }).ToList()
                        };

                        await _questionRepository.CreateAsync(question);
                    }

                    // Reload quiz with questions
                    createdQuiz = await _quizRepository.GetByIdWithQuestionsAsync(createdQuiz.Id);
                }

                return CreatedAtAction(
                    nameof(GetQuizById),
                    new { id = createdQuiz!.Id },
                    new ApiResponse<QuizDto>
                    {
                        Success = true,
                        Message = "Quiz created successfully",
                        Data = createdQuiz.ToDto()
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating quiz");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while creating the quiz",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<QuizDto>>> UpdateQuiz(int id, [FromBody] UpdateQuizDto updateQuizDto)
        {
            try
            {
                var existingQuiz = await _quizRepository.GetByIdAsync(id);
                if (existingQuiz == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Message = $"Quiz with ID {id} not found"
                    });
                }

                if (!string.IsNullOrEmpty(updateQuizDto.QuizName))
                {
                    existingQuiz.QuizName = updateQuizDto.QuizName;
                }

                var updatedQuiz = await _quizRepository.UpdateAsync(existingQuiz);

                return Ok(new ApiResponse<QuizDto>
                {
                    Success = true,
                    Message = "Quiz updated successfully",
                    Data = updatedQuiz!.ToDto()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating quiz with ID {QuizId}", id);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while updating the quiz",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteQuiz(int id)
        {
            try
            {
                var deleted = await _quizRepository.DeleteAsync(id);
                if (!deleted)
                {
                    return NotFound(new ErrorResponse
                    {
                        Message = $"Quiz with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Quiz deleted successfully",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting quiz with ID {QuizId}", id);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while deleting the quiz",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}