using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizApi.DAL;
using QuizApi.Domain;
using QuizApi.DTOs;
using System.Security.Claims;

namespace QuizApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultController : ControllerBase
    {
        private readonly ILogger<ResultController> _logger;
        private readonly QuizDbContext _db;
        public ResultController(QuizDbContext db, ILogger<ResultController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // RESULTS METHODS

        // POST /api/quiz/{quizId}/results  [Authorize]
        [HttpPost("{quizId:int}/results")]
        [Authorize]
        public async Task<ActionResult<ResultReadDto>> SubmitResult(int quizId, [FromBody] ResultCreateDto dto, CancellationToken ct)
        {
            if (dto.QuizId != quizId)
            {
                ModelState.AddModelError("QuizId", "QuizId must match the route quizId.");
                return ValidationProblem(ModelState);
            }

            if (dto.TotalQuestions < 1)
            {
                ModelState.AddModelError("TotalQuestions", "TotalQuestions must be at least 1.");
                return ValidationProblem(ModelState);
            }
            if (dto.CorrectCount < 0 || dto.CorrectCount > dto.TotalQuestions)
            {
                ModelState.AddModelError("CorrectCount", "CorrectCount must be between 0 and TotalQuestions.");
                return ValidationProblem(ModelState);
            }

            var exists = await _db.Quizzes.AnyAsync(q => q.QuizId == quizId, ct);
            if (!exists) return NotFound();

            var result = new Result
            {
                UserId = dto.UserId,
                QuizId = dto.QuizId,
                CorrectCount = dto.CorrectCount,
                TotalQuestions = dto.TotalQuestions,
                CompletedAt = DateTime.UtcNow
            };

            _db.Results.Add(result);
            await _db.SaveChangesAsync(ct);

            var read = new ResultReadDto(
                result.ResultId,
                result.UserId,
                result.QuizId,
                result.CorrectCount,
                result.TotalQuestions,
                result.CompletedAt,
                result.Percentage);

            return CreatedAtAction(nameof(GetResult), new { quizId, resultId = result.ResultId }, read);
        }

        // GET /api/quiz/{quizId}/results/{resultId}  [Authorize]
        [HttpGet("{quizId:int}/results/{resultId:int}")]
        [Authorize]
        public async Task<ActionResult<ResultReadDto>> GetResult(int quizId, int resultId, CancellationToken ct)
        {
            var r = await _db.Results.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ResultId == resultId && x.QuizId == quizId, ct);

            if (r is null) return NotFound();

            var read = new ResultReadDto(
                r.ResultId, r.UserId, r.QuizId,
                r.CorrectCount, r.TotalQuestions,
                r.CompletedAt, r.Percentage);

            return Ok(read);
        }
    }
}