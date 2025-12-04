using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApi.DAL;
using QuizApi.Domain;
using QuizApi.DTOs;
using System.Security.Claims;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QuizApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionController : ControllerBase
    {
        private readonly QuizDbContext _db;

        public QuestionController(QuizDbContext db)
        {
            _db = db;
        }

        // POST /api/quiz/{quizId}/questions [Authorize] - add question under a quiz
        [HttpPost("{quizId:int}/questions")]
        [Authorize]
        public async Task<ActionResult<QuestionReadDto>> AddQuestion(int quizId, [FromBody] QuestionCreateDto dto, CancellationToken ct)
        {
            if (dto.QuizId != quizId)
            {
                ModelState.AddModelError("QuizId", "QuizId must match the route quizId.");
                return ValidationProblem(ModelState);
            }

            if (string.IsNullOrWhiteSpace(dto.Text))
            {
                ModelState.AddModelError("Text", "Text is required.");
                return ValidationProblem(ModelState);
            }

            // Verify quiz exists and caller is the owner
            var quiz = await _db.Quizzes.FirstOrDefaultAsync(q => q.QuizId == quizId, ct);
            if (quiz is null) return NotFound();

            var ownerClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(ownerClaim) &&
                !string.IsNullOrEmpty(quiz.OwnerId) &&
                quiz.OwnerId != ownerClaim)
            {
                return Forbid();
            }

            var q = new Question { Text = dto.Text.Trim(), QuizId = quizId };
            _db.Questions.Add(q);
            await _db.SaveChangesAsync(ct);

            var read = new QuestionReadDto(q.QuestionId, q.Text, q.QuizId);
            return CreatedAtAction(nameof(GetQuestion), new { quizId, questionId = q.QuestionId }, read);
        }

        // GET /api/quiz/{quizId}/questions/{questionId}  [Authorize/owner or published]
        [HttpGet("{quizId:int}/questions/{questionId:int}")]
        [Authorize]
        public async Task<ActionResult<QuestionReadDto>> GetQuestion(int quizId, int questionId, CancellationToken ct)
        {
            var q = await _db.Questions
                .Include(x => x.Quiz)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.QuestionId == questionId && x.QuizId == quizId, ct);

            if (q is null) return NotFound();

            // Owner can view drafts; others only if quiz is published
            var ownerClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isOwner = !string.IsNullOrEmpty(ownerClaim) &&
                          !string.IsNullOrEmpty(q.Quiz?.OwnerId) &&
                          q.Quiz.OwnerId == ownerClaim;
            if (!isOwner && q.Quiz is not null && !q.Quiz.IsPublished)
            {
                return Forbid();
            }

            return Ok(new QuestionReadDto(q.QuestionId, q.Text, q.QuizId));
        }

        // PUT /api/quiz/{quizId}/questions/{questionId}  [Authorize] - update question text
        [HttpPut("{quizId:int}/questions/{questionId:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateQuestion(int quizId, int questionId, [FromBody] QuestionUpdateDto dto, CancellationToken ct)
        {
            var q = await _db.Questions
                .Include(x => x.Quiz)
                .FirstOrDefaultAsync(x => x.QuestionId == questionId && x.QuizId == quizId, ct);

            if (q is null) return NotFound();

            // Only quiz owner can modify
            var ownerClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(ownerClaim) &&
                !string.IsNullOrEmpty(q.Quiz!.OwnerId) &&
                q.Quiz.OwnerId != ownerClaim)
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(dto.Text))
            {
                ModelState.AddModelError("Text", "Text is required.");
                return ValidationProblem(ModelState);
            }

            q.Text = dto.Text.Trim();
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }

        // DELETE /api/quiz/{quizId}/questions/{questionId}  [Authorize] - remove question
        [HttpDelete("{quizId:int}/questions/{questionId:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteQuestion(int quizId, int questionId, CancellationToken ct)
        {
            var q = await _db.Questions
                .Include(x => x.Quiz)
                .FirstOrDefaultAsync(x => x.QuestionId == questionId && x.QuizId == quizId, ct);
            if (q is null) return NotFound();

            var ownerClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(ownerClaim) &&
                !string.IsNullOrEmpty(q.Quiz!.OwnerId) &&
                q.Quiz.OwnerId != ownerClaim)
            {
                return Forbid();
            }

            _db.Questions.Remove(q);
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}
