using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApi.DAL;
using QuizApi.DTOs;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;
using System;
using QuizApi.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace QuizApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ResultController : ControllerBase
    {
        private readonly QuizDbContext _db;

        public ResultController(QuizDbContext db)
        {
            _db = db;
        }

        // GET: /api/Result/{resultId}/full
        // Full result: summary + quiz structure + all answers
        [HttpGet("{resultId:int}/full")]
        public async Task<ActionResult<FullResultDto>> GetFullResult(
            int resultId,
            CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _db.Results
                .Include(r => r.Quiz)
                    .ThenInclude(q => q.Questions)
                        .ThenInclude(q => q.Options)
                .Include(r => r.Answers)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ResultId == resultId && r.UserId == userId, ct);

            if (result is null) return NotFound();

            // Map to FullResultDto
            var resultDto = new ResultReadDto(
                ResultId:       result.ResultId,
                UserId:         result.UserId,
                QuizId:         result.QuizId,
                QuizTitle:      result.Quiz?.Title ?? string.Empty,
                SubjectCode:    result.Quiz?.SubjectCode ?? string.Empty,
                CorrectCount:   result.CorrectCount,
                TotalQuestions: result.TotalQuestions,
                CompletedAt:    result.CompletedAt,
                Percentage:     result.Percentage
            );

            
            var quizEntity = result.Quiz!;

            var quizDto = new TakeQuizDto
            {
                Id          = quizEntity.QuizId,
                SubjectCode = quizEntity.SubjectCode ?? string.Empty,
                Title       = quizEntity.Title,
                Description = quizEntity.Description ?? string.Empty,
                ImageUrl    = quizEntity.ImageUrl ?? string.Empty,
                IsPublished = quizEntity.IsPublished,
                Questions   = quizEntity.Questions
                    .OrderBy(q => q.QuestionId)
                    .Select(q => new TakeQuestionDto
                    {
                        Id       = q.QuestionId,
                        Text     = q.Text,
                        ImageUrl = null,   
                        Points   = 1,
                        Options  = q.Options
                            .OrderBy(o => o.OptionID)
                            .Select(o => new TakeOptionDto
                            {
                                Id        = o.OptionID,
                                Text      = o.Text,
                                IsCorrect = o.IsCorrect
                            })
                            .ToList()
                    })
                    .ToList()
            };

        
            var answersDict = result.Answers
                .ToDictionary(a => a.QuestionId, a => a.OptionId);

            var fullDto = new FullResultDto
            {
                Result  = resultDto,
                Quiz    = quizDto,
                Answers = answersDict
            };

            return Ok(fullDto);
        }

        // POST /api/Result
        // Submit a quiz result (includes per-question answers)
        [HttpPost]
        public async Task<ActionResult<ResultReadDto>> SubmitResult(
            [FromBody] ResultCreateDto dto,
            CancellationToken ct)
        {
            if (dto.QuizId <= 0)
            {
                ModelState.AddModelError(nameof(dto.QuizId), "QuizId must be a positive integer.");
                return ValidationProblem(ModelState);
            }

            var quiz = await _db.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(q => q.QuizId == dto.QuizId, ct);
            if (quiz is null) return NotFound();

            if (!quiz.IsPublished)
            {
                return Forbid();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var questions = quiz.Questions.OrderBy(q => q.QuestionId).ToList();
            var totalQuestions = questions.Count;

            if (totalQuestions < 1)
            {
                ModelState.AddModelError("TotalQuestions", "Quiz has no questions.");
                return ValidationProblem(ModelState);
            }

            var answersPayload = dto.Answers ?? new Dictionary<int, int>(); // questionId -> optionId
            var resultAnswers = new List<ResultAnswer>();
            var correctCount = 0;

            foreach (var question in questions)
            {
                if (!answersPayload.TryGetValue(question.QuestionId, out var chosenOptionId))
                {
                    continue; // unanswered
                }

                var chosenOption = question.Options.FirstOrDefault(o => o.OptionID == chosenOptionId);
                if (chosenOption is null)
                {
                    ModelState.AddModelError("Answers", $"Option {chosenOptionId} does not belong to question {question.QuestionId}.");
                    return ValidationProblem(ModelState);
                }

                resultAnswers.Add(new ResultAnswer
                {
                    QuestionId = question.QuestionId,
                    OptionId   = chosenOption.OptionID
                });

                if (chosenOption.IsCorrect)
                {
                    correctCount += 1;
                }
            }

            var result = new Result
            {
                UserId         = userId,
                QuizId         = dto.QuizId,
                CorrectCount   = correctCount,
                TotalQuestions = totalQuestions,
                CompletedAt    = DateTime.UtcNow
            };

            _db.Results.Add(result);
            await _db.SaveChangesAsync(ct); // need ResultId for answers

            if (resultAnswers.Count > 0)
            {
                foreach (var answer in resultAnswers)
                {
                    answer.ResultId = result.ResultId;
                    _db.ResultAnswers.Add(answer);
                }

                await _db.SaveChangesAsync(ct);
            }

            var readQuiz = await _db.Quizzes.AsNoTracking()
                .FirstOrDefaultAsync(q => q.QuizId == dto.QuizId, ct);

            var read = new ResultReadDto(
                ResultId:       result.ResultId,
                UserId:         result.UserId,
                QuizId:         result.QuizId,
                QuizTitle:      readQuiz?.Title ?? string.Empty,
                SubjectCode:    readQuiz?.SubjectCode ?? string.Empty,
                CorrectCount:   result.CorrectCount,
                TotalQuestions: result.TotalQuestions,
                CompletedAt:    result.CompletedAt,
                Percentage:     result.Percentage
            );

            return CreatedAtAction(
                nameof(GetResultSummary),
                new { resultId = result.ResultId },
                read
            );
        }

        // GET /api/Result/{resultId}
        // Summary for a single result (no answers)
        [HttpGet("{resultId:int}")]
        public async Task<ActionResult<ResultReadDto>> GetResultSummary(
            int resultId,
            CancellationToken ct)
        {
            var r = await _db.Results.AsNoTracking()
                .Join(
                    _db.Quizzes,
                    res => res.QuizId,
                    q   => q.QuizId,
                    (res, q) => new { res, q }
                )
                .FirstOrDefaultAsync(x => x.res.ResultId == resultId, ct);

            if (r is null) return NotFound();

            // Only allow owner to see summary
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || r.res.UserId != userId)
            {
                return Forbid();
            }

            var read = new ResultReadDto(
                r.res.ResultId,
                r.res.UserId,
                r.res.QuizId,
                r.q.Title,
                r.q.SubjectCode,
                r.res.CorrectCount,
                r.res.TotalQuestions,
                r.res.CompletedAt,
                r.res.Percentage
            );

            return Ok(read);
        }

        // GET /api/Result/my
        // All results for the current logged-in user.
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<ResultReadDto>>> GetMyResults(
            CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var items = await _db.Results
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CompletedAt)
                .Join(
                    _db.Quizzes,
                    r => r.QuizId,
                    q => q.QuizId,
                    (r, q) => new ResultReadDto(
                        r.ResultId,
                        r.UserId,
                        r.QuizId,
                        q.Title,
                        q.SubjectCode,
                        r.CorrectCount,
                        r.TotalQuestions,
                        r.CompletedAt,
                        r.Percentage
                    )
                )
                .ToListAsync(ct);

            return Ok(items);
        }
    }
}
