using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizApi.DAL;
using QuizApi.Domain;
using QuizApi.DTOs;
using System.Security.Claims;
using System.Linq;

namespace QuizApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OptionController : ControllerBase
    {
        // CONTROLLER
        private readonly QuizDbContext _db;

        // CONSTRUCTOR
        public OptionController(QuizDbContext db)
        {
            _db = db;
        }


        // OPTIONS METHODS

        // POST /api/quiz/{quizId}/questions/{questionId}/options [Authorize]
        // Create/add option to question. User has to be logged in. 
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

            // Ensure question exists and caller is the quiz owner
            var question = await _db.Questions
                .Include(q => q.Quiz)
                .FirstOrDefaultAsync(x => x.QuestionId == questionId && x.QuizId == quizId, ct);
            if (question is null) return NotFound();

            var ownerClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(ownerClaim) &&
                !string.IsNullOrEmpty(question.Quiz!.OwnerId) &&
                question.Quiz.OwnerId != ownerClaim)
            {
                return Forbid();
            }

            var o = new Option { QuestionId = questionId, Text = dto.Text.Trim(), IsCorrect = dto.IsCorrect };
            _db.Options.Add(o);
            await _db.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(GetOption), new { quizId, questionId, optionId = o.OptionID },
                new OptionReadDto(o.OptionID, o.Text, o.IsCorrect, o.QuestionId));
            // Returns 201 Created with the created option
        }

        // GET /api/quiz/{quizId}/questions/{questionId}/options/{optionId} [AllowAnonymous]
        // Read/get option. User has to be logged in.
        [HttpGet("{quizId:int}/questions/{questionId:int}/options/{optionId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<OptionReadDto>> GetOption(int quizId, int questionId, int optionId, CancellationToken ct)
        {
            var o = await _db.Options.AsNoTracking()
                .Include(x => x.Question)
                .ThenInclude(q => q!.Quiz)
                .FirstOrDefaultAsync(x => x.OptionID == optionId && x.QuestionId == questionId, ct);

            if (o is null) return NotFound();

            // Only owner or published quiz can read through this controller
            var ownerClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isOwner = !string.IsNullOrEmpty(ownerClaim) &&
                          !string.IsNullOrEmpty(o.Question!.Quiz!.OwnerId) &&
                          o.Question.Quiz.OwnerId == ownerClaim;
            if (!isOwner && !o.Question.Quiz.IsPublished)
            {
                return Forbid();
            }
            return Ok(new OptionReadDto(o.OptionID, o.Text, o.IsCorrect, o.QuestionId));
            // Returns 200 OK with the option
        }

        // PUT /api/quiz/{quizId}/questions/{questionId}/options/{optionId} [Authorize]
        // Method for updating option. User has to be logged in.
        [HttpPut("{quizId:int}/questions/{questionId:int}/options/{optionId:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateOption(int quizId, int questionId, int optionId, [FromBody] OptionUpdateDto dto, CancellationToken ct)
        {
            var o = await _db.Options
                .Include(x => x.Question)
                .ThenInclude(q => q!.Quiz)
                .FirstOrDefaultAsync(x => x.OptionID == optionId && x.QuestionId == questionId, ct);

            if (o is null) return NotFound();

            var ownerClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(ownerClaim) &&
                !string.IsNullOrEmpty(o.Question!.Quiz!.OwnerId) &&
                o.Question.Quiz.OwnerId != ownerClaim)
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(dto.Text))
            {
                ModelState.AddModelError("Text", "Text is required.");
                return ValidationProblem(ModelState);
            }

            o.Text = dto.Text.Trim();
            o.IsCorrect = dto.IsCorrect;
            await _db.SaveChangesAsync(ct);
            return NoContent();
            // Returns 204 No Content
        }

        // DELETE /api/quiz/{quizId}/questions/{questionId}/options/{optionId}  [Authorize]
        // Method for deleting option. User has to be logged in.
        [HttpDelete("{quizId:int}/questions/{questionId:int}/options/{optionId:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteOption(int quizId, int questionId, int optionId, CancellationToken ct)
        {
            var o = await _db.Options
                .Include(x => x.Question)
                .ThenInclude(q => q!.Quiz)
                .FirstOrDefaultAsync(x => x.OptionID == optionId && x.QuestionId == questionId, ct);
            if (o is null) return NotFound();

            var ownerClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(ownerClaim) &&
                !string.IsNullOrEmpty(o.Question!.Quiz!.OwnerId) &&
                o.Question.Quiz.OwnerId != ownerClaim)
            {
                return Forbid();
            }

            _db.Options.Remove(o);
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }

    }
}
