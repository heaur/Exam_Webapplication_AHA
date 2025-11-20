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
    public class QuizController : ControllerBase
    {
        private readonly ILogger<QuizController> _logger;
        private readonly QuizDbContext _db;

        public QuizController(ILogger<QuizController> logger, QuizDbContext db)
        {
            _logger = logger;
            _db = db;
        }


        // QUIZ METHODS

        // POST /api/quiz.Create a new quiz. User has to be authenticated.  [Authorize]
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<QuizReadDto>> CreateQuiz([FromBody] QuizCreateDto dto, CancellationToken ct)
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                ModelState.AddModelError("Title", "Title is required.");
                return ValidationProblem(ModelState);
                // Returns 400 Bad Request with validation errors if title is missing
            }

            // Read user id from claims as string and store it directly (Quiz.OwnerId is a string)
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Create new quiz entity
            var now = DateTime.UtcNow;
            var entity = new Quiz
            {
                Title = dto.Title.Trim(),
                Description = dto.Description?.Trim(),
                CreatedAt = now,
                UpdatedAt = null,
                IsPublished = false,
                PublishedAt = null,
                OwnerId = ownerId
            };

            // Save to database
            _db.Quizzes.Add(entity);
            await _db.SaveChangesAsync(ct);

            var read = new QuizReadDto(
                entity.QuizId,
                entity.Title,
                entity.Description,
                entity.CreatedAt,
                entity.UpdatedAt,
                entity.IsPublished,
                entity.PublishedAt,
                entity.OwnerId,
                0);

            return CreatedAtAction(nameof(GetQuiz), new { id = entity.QuizId }, read);
            // Returns 201 Created with the created quiz
        }

        // GET /api/quiz/{id}. Get quiz by ID. User does not have to be logged in. [AllowAnonymous]
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<QuizReadDto>> GetQuiz(int id, CancellationToken ct)
        {
            // Include questions to count them
            var quiz = await _db.Quizzes
                .Include(q => q.Questions)
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.QuizId == id, ct);

            // If not found, return 404
            if (quiz is null) return NotFound();

            var read = new QuizReadDto(
                quiz.QuizId,
                quiz.Title,
                quiz.Description,
                quiz.CreatedAt,
                quiz.UpdatedAt,
                quiz.IsPublished,
                quiz.PublishedAt,
                quiz.OwnerId,
                quiz.Questions.Count);

            return Ok(read);
            // Returns 200 OK with the quiz data
        }

        // GET /api/quiz. Lists quizzes. User does not have to be logged in. [AllowAnonymous]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<QuizReadDto>>> ListQuizzes(
            [FromQuery] string? search,
            CancellationToken ct)
        {
            var query = _db.Quizzes.Include(q => q.Questions).AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(q => q.Title.Contains(search));

            var items = await query
                .OrderByDescending(q => q.CreatedAt)
                .Select(q => new QuizReadDto(
                    q.QuizId,
                    q.Title,
                    q.Description,
                    q.CreatedAt,
                    q.UpdatedAt,
                    q.IsPublished,
                    q.PublishedAt,
                    q.OwnerId,
                    q.Questions.Count))
                .ToListAsync(ct);

            return Ok(items);
            // Returns 200 OK with the list of quizzes
        }

        // PUT /api/quiz/{id}. Method for updating quiz. User has to be logged in. Only owner can edit.[Authorize]
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateQuiz(int id, [FromBody] QuizUpdateDto dto, CancellationToken ct)
        {
            var quiz = await _db.Quizzes.FirstOrDefaultAsync(q => q.QuizId == id, ct);
            if (quiz is null) return NotFound();

            // Owner check
            var ownerClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(ownerClaim) && !string.IsNullOrEmpty(quiz.OwnerId) && quiz.OwnerId != ownerClaim)
                return Forbid();

            if (!string.IsNullOrWhiteSpace(dto.Title))
                quiz.Title = dto.Title.Trim();

            quiz.Description = dto.Description?.Trim();

            if (dto.IsPublished.HasValue)
            {
                if (dto.IsPublished.Value && !quiz.IsPublished)
                {
                    quiz.IsPublished = true;
                    quiz.PublishedAt = DateTime.UtcNow;
                }
                else if (!dto.IsPublished.Value && quiz.IsPublished)
                {
                    quiz.IsPublished = false;
                }
            }

            quiz.UpdatedAt = DateTime.UtcNow;
            // Save changes
            await _db.SaveChangesAsync(ct);
            return NoContent();
            // Returns 204 No Content on success
        }

        // DELETE /api/quiz/{id}. Deletes quiz. User has to be logged in.  [Authorize]
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteQuiz(int id, CancellationToken ct)
        {
            var quiz = await _db.Quizzes.FirstOrDefaultAsync(q => q.QuizId == id, ct);
            if (quiz is null) return NotFound();

            // Owner check
            var ownerClaim = User.FindFirstValue(ClaimTypes.NameIdentifier); // This is a string from Identity
            if (!string.IsNullOrEmpty(ownerClaim) && !string.IsNullOrEmpty(quiz.OwnerId) && quiz.OwnerId != ownerClaim)
            return Forbid();

            _db.Quizzes.Remove(quiz);
            // Save changes
            await _db.SaveChangesAsync(ct);
            return NoContent();
            // Returns 204 No Content on success
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

        // ======================= OPTIONS (nested) =======================

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

        // ======================= RESULTS =======================

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
