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
    public class OptionController : ControllerBase
    {
        private readonly QuizDbContext _db;

        public OptionController(QuizDbContext db)
        {
            _db = db;
        }


    // OPTIONS METHODS

        // POST /api/quiz/{quizId}/questions/{questionId}/options  [Authorize]
        [HttpPost("{quizId:int}/questions/{questionId:int}/options")]
        [Authorize]
        public async Task<ActionResult<OptionReadDto>> AddOption(int quizId, int questionId, [FromBody] OptionCreateDto dto, CancellationToken ct)
        {
            if (dto.QuestionId != questionId)
            {
                ModelState.AddModelError("QuestionId", "QuestionId must match the route questionId.");
                return ValidationProblem(ModelState);
            }

            if (string.IsNullOrWhiteSpace(dto.Text))
            {
                ModelState.AddModelError("Text", "Text is required.");
                return ValidationProblem(ModelState);
            }

            var question = await _db.Questions.FirstOrDefaultAsync(x => x.QuestionId == questionId && x.QuizId == quizId, ct);
            if (question is null) return NotFound();

            var o = new Option { QuestionId = questionId, Text = dto.Text.Trim(), IsCorrect = dto.IsCorrect };
            _db.Options.Add(o);
            await _db.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(GetOption), new { quizId, questionId, optionId = o.OptionID },
                new OptionReadDto(o.OptionID, o.Text, o.IsCorrect, o.QuestionId));
        }

        // GET /api/quiz/{quizId}/questions/{questionId}/options/{optionId}  [AllowAnonymous]
        [HttpGet("{quizId:int}/questions/{questionId:int}/options/{optionId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<OptionReadDto>> GetOption(int quizId, int questionId, int optionId, CancellationToken ct)
        {
            var o = await _db.Options.AsNoTracking()
                .FirstOrDefaultAsync(x => x.OptionID == optionId && x.QuestionId == questionId, ct);

            if (o is null) return NotFound();
            return Ok(new OptionReadDto(o.OptionID, o.Text, o.IsCorrect, o.QuestionId));
        }

        // PUT /api/quiz/{quizId}/questions/{questionId}/options/{optionId}  [Authorize]
        [HttpPut("{quizId:int}/questions/{questionId:int}/options/{optionId:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateOption(int quizId, int questionId, int optionId, [FromBody] OptionUpdateDto dto, CancellationToken ct)
        {
            var o = await _db.Options
                .Include(x => x.Question)
                .ThenInclude(q => q!.Quiz)
                .FirstOrDefaultAsync(x => x.OptionID == optionId && x.QuestionId == questionId, ct);

            if (o is null) return NotFound();

            if (string.IsNullOrWhiteSpace(dto.Text))
            {
                ModelState.AddModelError("Text", "Text is required.");
                return ValidationProblem(ModelState);
            }

            o.Text = dto.Text.Trim();
            o.IsCorrect = dto.IsCorrect;
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }

        // DELETE /api/quiz/{quizId}/questions/{questionId}/options/{optionId}  [Authorize]
        [HttpDelete("{quizId:int}/questions/{questionId:int}/options/{optionId:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteOption(int quizId, int questionId, int optionId, CancellationToken ct)
        {
            var o = await _db.Options.FirstOrDefaultAsync(x => x.OptionID == optionId && x.QuestionId == questionId, ct);
            if (o is null) return NotFound();

            _db.Options.Remove(o);
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }

    }
}