using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApi.DAL;
using QuizApi.DTOs;
using System.Security.Claims;
using System.Linq;
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

            // 1) Map the summary part
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

            // 2) Map the quiz part to the same TakeQuizDto as /api/quiz/{id}/take
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
                        ImageUrl = null,   // set to q.ImageUrl if you start using it
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

            // 3) Map answers: questionId -> optionId
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
    }
}
