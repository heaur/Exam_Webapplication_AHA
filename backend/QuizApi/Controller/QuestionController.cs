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
    public class QuestionController : ControllerBase
    {
        private readonly QuizDbContext _db;

        public QuestionController(QuizDbContext db)
        {
            _db = db;
        }
            
    //QUESTIONS METHODS

        // POST /api/quiz/{quizId}/questions. Creates/adds question. User has to be logged in. [Authorize]
        [HttpPost("{quizId:int}/questions")]
        [Authorize]
        public async Task<ActionResult<QuestionReadDto>> AddQuestion(int quizId, [FromBody] QuestionCreateDto dto, CancellationToken ct)
        {
            // Ensure body belongs to the route quiz
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

            var quizExists = await _db.Quizzes.AnyAsync(q => q.QuizId == quizId, ct);
            if (!quizExists) return NotFound();

            // Create and save question
            var q = new Question { Text = dto.Text.Trim(), QuizId = quizId };
            _db.Questions.Add(q);
            await _db.SaveChangesAsync(ct);

            var read = new QuestionReadDto(q.QuestionId, q.Text, q.QuizId);
            return CreatedAtAction(nameof(GetQuestion), new { quizId, questionId = q.QuestionId }, read);
            // Returns 201 Created with the created question
        }

        // GET /api/quiz/{quizId}/questions/{questionId}  [AllowAnonymous]
        [HttpGet("{quizId:int}/questions/{questionId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<QuestionReadDto>> GetQuestion(int quizId, int questionId, CancellationToken ct)
        {
            var q = await _db.Questions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.QuestionId == questionId && x.QuizId == quizId, ct);

            if (q is null) return NotFound();
            return Ok(new QuestionReadDto(q.QuestionId, q.Text, q.QuizId));
        }

        // PUT /api/quiz/{quizId}/questions/{questionId}  [Authorize]
        [HttpPut("{quizId:int}/questions/{questionId:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateQuestion(int quizId, int questionId, [FromBody] QuestionUpdateDto dto, CancellationToken ct)
        {
            var q = await _db.Questions
                .Include(x => x.Quiz)
                .FirstOrDefaultAsync(x => x.QuestionId == questionId && x.QuizId == quizId, ct);

            if (q is null) return NotFound();

            if (string.IsNullOrWhiteSpace(dto.Text))
            {
                ModelState.AddModelError("Text", "Text is required.");
                return ValidationProblem(ModelState);
            }

            q.Text = dto.Text.Trim();
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }

        // DELETE /api/quiz/{quizId}/questions/{questionId}  [Authorize]
        [HttpDelete("{quizId:int}/questions/{questionId:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteQuestion(int quizId, int questionId, CancellationToken ct)
        {
            var q = await _db.Questions.FirstOrDefaultAsync(x => x.QuestionId == questionId && x.QuizId == quizId, ct);
            if (q is null) return NotFound();

            _db.Questions.Remove(q);
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }


    }

}