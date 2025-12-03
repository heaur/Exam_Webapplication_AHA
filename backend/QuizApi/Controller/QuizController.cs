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

        // ===========================================================
        //                       QUIZ (CRUD)
        // ===========================================================

        // POST /api/quiz
        // Create a new quiz. User has to be authenticated.
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<QuizReadDto>> CreateQuiz(
            [FromBody] QuizCreateDto dto,
            CancellationToken ct)
        {
            // 1) Valider tittel
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                ModelState.AddModelError(nameof(dto.Title), "Title is required.");
                return ValidationProblem(ModelState);
            }

            // 2) Normaliser fagkode
            var subjectCode = string.IsNullOrWhiteSpace(dto.SubjectCode)
                ? "OTHER"
                : dto.SubjectCode.Trim().ToUpper();

            // 3) Rydd opp i tekstfelter
            var description = string.IsNullOrWhiteSpace(dto.Description)
                ? null
                : dto.Description.Trim();

            var imageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl)
                ? null
                : dto.ImageUrl.Trim();

            // 4) Hent innlogget bruker som eier
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var now = DateTime.UtcNow;

            // 5) Lag selve Quiz-entiteten
            var quizEntity = new Quiz
            {
                Title       = dto.Title.Trim(),
                SubjectCode = subjectCode,
                Description = description,
                ImageUrl    = imageUrl,
                CreatedAt   = now,
                UpdatedAt   = null,
                IsPublished = false,
                PublishedAt = null,
                OwnerId     = ownerId
            };

            _db.Quizzes.Add(quizEntity);
            await _db.SaveChangesAsync(ct); // trenger QuizId før vi kan lage spørsmål

            // 6) Lag spørsmål + alternativer hvis noe ble sendt inn
            if (dto.Questions != null && dto.Questions.Count > 0)
            {
                foreach (var q in dto.Questions)
                {
                    if (string.IsNullOrWhiteSpace(q.Text))
                        continue;

                    var questionEntity = new Question
                    {
                        QuizId = quizEntity.QuizId,
                        Text   = q.Text.Trim()
                    };

                    _db.Questions.Add(questionEntity);
                    await _db.SaveChangesAsync(ct); // får QuestionId

                    if (q.Options != null && q.Options.Count > 0)
                    {
                        foreach (var o in q.Options)
                        {
                            if (string.IsNullOrWhiteSpace(o.Text))
                                continue;

                            var optionEntity = new Option
                            {
                                QuestionId = questionEntity.QuestionId,
                                Text       = o.Text.Trim(),
                                IsCorrect  = o.IsCorrect
                            };

                            _db.Options.Add(optionEntity);
                        }

                        await _db.SaveChangesAsync(ct);
                    }
                }
            }

            // 7) Bygg read-DTO til respons
            var read = new QuizReadDto(
                Id:            quizEntity.QuizId,
                Title:         quizEntity.Title,
                SubjectCode:   quizEntity.SubjectCode,
                Description:   quizEntity.Description,
                ImageUrl:      quizEntity.ImageUrl,
                CreatedAt:     quizEntity.CreatedAt,
                UpdatedAt:     quizEntity.UpdatedAt,
                IsPublished:   quizEntity.IsPublished,
                PublishedAt:   quizEntity.PublishedAt,
                OwnerId:       quizEntity.OwnerId,
                QuestionCount: await _db.Questions.CountAsync(q => q.QuizId == quizEntity.QuizId, ct)
            );

            return CreatedAtAction(nameof(GetQuiz), new { id = quizEntity.QuizId }, read);
        }

        // GET /api/quiz/{id}
        // Get quiz by ID (summary with question count only).
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<QuizReadDto>> GetQuiz(int id, CancellationToken ct)
        {
            var quiz = await _db.Quizzes
                .Include(q => q.Questions)
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.QuizId == id, ct);

            if (quiz is null) return NotFound();

            var read = new QuizReadDto(
                Id: quiz.QuizId,
                Title: quiz.Title,
                SubjectCode: quiz.SubjectCode,
                Description: quiz.Description,
                ImageUrl: quiz.ImageUrl,
                CreatedAt: quiz.CreatedAt,
                UpdatedAt: quiz.UpdatedAt,
                IsPublished: quiz.IsPublished,
                PublishedAt: quiz.PublishedAt,
                OwnerId: quiz.OwnerId,
                QuestionCount: quiz.Questions.Count
            );

            return Ok(read);
        }

        // GET /api/quiz/{id}/take
        // Full quiz for "take view" (inkl. questions + options).
        [HttpGet("{id:int}/take")]
        [AllowAnonymous]
        public async Task<ActionResult<TakeQuizDto>> GetQuizForTake(
            int id,
            CancellationToken ct)
        {
            var quiz = await _db.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Options)
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.QuizId == id, ct);

            if (quiz is null) return NotFound();

            var dto = new TakeQuizDto
            {
                Id          = quiz.QuizId,
                SubjectCode = quiz.SubjectCode ?? string.Empty,
                Title       = quiz.Title,
                Description = quiz.Description ?? string.Empty,
                ImageUrl    = quiz.ImageUrl ?? string.Empty,
                IsPublished = quiz.IsPublished,
                Questions   = quiz.Questions
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

            return Ok(dto);
        }

        // GET /api/quiz
        // List quizzes for homepage / browse.
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<QuizReadDto>>> ListQuizzes(
            [FromQuery] string? search,
            CancellationToken ct)
        {
            var query = _db.Quizzes
                .Include(q => q.Questions)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(q => q.Title.Contains(search));

            var items = await query
                .OrderByDescending(q => q.CreatedAt)
                .Select(q => new QuizReadDto(
                    q.QuizId,
                    q.Title,
                    q.SubjectCode,
                    q.Description,
                    q.ImageUrl,
                    q.CreatedAt,
                    q.UpdatedAt,
                    q.IsPublished,
                    q.PublishedAt,
                    q.OwnerId,
                    q.Questions.Count
                ))
                .ToListAsync(ct);

            return Ok(items);
        }

        // GET /api/quiz/my
        // Alle quizer som innlogget bruker eier (til "Min profil").
        [HttpGet("my")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<QuizReadDto>>> GetMyQuizzes(
            CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var items = await _db.Quizzes
                .Where(q => q.OwnerId == userId)
                .Include(q => q.Questions)
                .AsNoTracking()
                .OrderByDescending(q => q.CreatedAt)
                .Select(q => new QuizReadDto(
                    q.QuizId,
                    q.Title,
                    q.SubjectCode,
                    q.Description,
                    q.ImageUrl,
                    q.CreatedAt,
                    q.UpdatedAt,
                    q.IsPublished,
                    q.PublishedAt,
                    q.OwnerId,
                    q.Questions.Count
                ))
                .ToListAsync(ct);

            return Ok(items);
        }

        // PUT /api/quiz/{id}
        // Update quiz metadata. Only owner can edit.
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateQuiz(
            int id,
            [FromBody] QuizUpdateDto dto,
            CancellationToken ct)
        {
            var quiz = await _db.Quizzes.FirstOrDefaultAsync(q => q.QuizId == id, ct);
            if (quiz is null) return NotFound();

            var ownerClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(ownerClaim) &&
                !string.IsNullOrEmpty(quiz.OwnerId) &&
                quiz.OwnerId != ownerClaim)
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                ModelState.AddModelError(nameof(dto.Title), "Title is required.");
                return ValidationProblem(ModelState);
            }

            var subjectCode = string.IsNullOrWhiteSpace(dto.SubjectCode)
                ? "OTHER"
                : dto.SubjectCode.Trim().ToUpper();

            var description = string.IsNullOrWhiteSpace(dto.Description)
                ? null
                : dto.Description.Trim();

            var imageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl)
                ? null
                : dto.ImageUrl.Trim();

            quiz.Title       = dto.Title.Trim();
            quiz.SubjectCode = subjectCode;
            quiz.Description = description;
            quiz.ImageUrl    = imageUrl;

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
                    quiz.PublishedAt = null;
                }
            }

            quiz.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return NoContent();
        }

        // DELETE /api/quiz/{id}
        // Delete quiz. Only owner can delete.
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteQuiz(int id, CancellationToken ct)
        {
            var quiz = await _db.Quizzes.FirstOrDefaultAsync(q => q.QuizId == id, ct);
            if (quiz is null) return NotFound();

            var ownerClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(ownerClaim) &&
                !string.IsNullOrEmpty(quiz.OwnerId) &&
                quiz.OwnerId != ownerClaim)
            {
                return Forbid();
            }

            _db.Quizzes.Remove(quiz);
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }

        // ===========================================================
        //                       QUESTIONS
        // ===========================================================

        [HttpPost("{quizId:int}/questions")]
        [Authorize]
        public async Task<ActionResult<QuestionReadDto>> AddQuestion(
            int quizId,
            [FromBody] QuestionCreateDto dto,
            CancellationToken ct)
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

            var quizExists = await _db.Quizzes.AnyAsync(q => q.QuizId == quizId, ct);
            if (!quizExists) return NotFound();

            var q = new Question { Text = dto.Text.Trim(), QuizId = quizId };
            _db.Questions.Add(q);
            await _db.SaveChangesAsync(ct);

            var read = new QuestionReadDto(q.QuestionId, q.Text, q.QuizId);
            return CreatedAtAction(nameof(GetQuestion), new { quizId, questionId = q.QuestionId }, read);
        }

        [HttpGet("{quizId:int}/questions/{questionId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<QuestionReadDto>> GetQuestion(
            int quizId,
            int questionId,
            CancellationToken ct)
        {
            var q = await _db.Questions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.QuestionId == questionId && x.QuizId == quizId, ct);

            if (q is null) return NotFound();
            return Ok(new QuestionReadDto(q.QuestionId, q.Text, q.QuizId));
        }

        [HttpPut("{quizId:int}/questions/{questionId:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateQuestion(
            int quizId,
            int questionId,
            [FromBody] QuestionUpdateDto dto,
            CancellationToken ct)
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

        [HttpDelete("{quizId:int}/questions/{questionId:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteQuestion(
            int quizId,
            int questionId,
            CancellationToken ct)
        {
            var q = await _db.Questions.FirstOrDefaultAsync(x => x.QuestionId == questionId && x.QuizId == quizId, ct);
            if (q is null) return NotFound();

            _db.Questions.Remove(q);
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }

        // ===========================================================
        //                       OPTIONS
        // ===========================================================

        [HttpPost("{quizId:int}/questions/{questionId:int}/options")]
        [Authorize]
        public async Task<ActionResult<OptionReadDto>> AddOption(
            int quizId,
            int questionId,
            [FromBody] OptionCreateDto dto,
            CancellationToken ct)
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

            var question = await _db.Questions
                .FirstOrDefaultAsync(x => x.QuestionId == questionId && x.QuizId == quizId, ct);
            if (question is null) return NotFound();

            var o = new Option { QuestionId = questionId, Text = dto.Text.Trim(), IsCorrect = dto.IsCorrect };
            _db.Options.Add(o);
            await _db.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(GetOption),
                new { quizId, questionId, optionId = o.OptionID },
                new OptionReadDto(o.OptionID, o.Text, o.IsCorrect, o.QuestionId));
        }

        [HttpGet("{quizId:int}/questions/{questionId:int}/options/{optionId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<OptionReadDto>> GetOption(
            int quizId,
            int questionId,
            int optionId,
            CancellationToken ct)
        {
            var o = await _db.Options.AsNoTracking()
                .FirstOrDefaultAsync(x => x.OptionID == optionId && x.QuestionId == questionId, ct);

            if (o is null) return NotFound();
            return Ok(new OptionReadDto(o.OptionID, o.Text, o.IsCorrect, o.QuestionId));
        }

        [HttpPut("{quizId:int}/questions/{questionId:int}/options/{optionId:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateOption(
            int quizId,
            int questionId,
            int optionId,
            [FromBody] OptionUpdateDto dto,
            CancellationToken ct)
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

        [HttpDelete("{quizId:int}/questions/{questionId:int}/options/{optionId:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteOption(
            int quizId,
            int questionId,
            int optionId,
            CancellationToken ct)
        {
            var o = await _db.Options.FirstOrDefaultAsync(x => x.OptionID == optionId && x.QuestionId == questionId, ct);
            if (o is null) return NotFound();

            _db.Options.Remove(o);
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }

        // ===========================================================
        //                       RESULTS
        // ===========================================================

        [HttpPost("{quizId:int}/results")]
        [Authorize]
        public async Task<ActionResult<ResultReadDto>> SubmitResult(
            int quizId,
            [FromBody] ResultCreateDto dto,
            CancellationToken ct)
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

            // Hent innlogget bruker fra cookie/claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = new Result
            {
                UserId         = userId,
                QuizId         = dto.QuizId,
                CorrectCount   = dto.CorrectCount,
                TotalQuestions = dto.TotalQuestions,
                CompletedAt    = DateTime.UtcNow
                // Percentage er read-only (NotMapped) -> beregnes automatisk
            };

            _db.Results.Add(result);
            await _db.SaveChangesAsync(ct); // trenger ResultId før vi lagrer answers

            // lagre alle brukerens svar (én rad per spørsmål)
            if (dto.Answers != null && dto.Answers.Count > 0)
            {
                foreach (var kvp in dto.Answers)
                {
                    var questionId = kvp.Key;
                    var optionId   = kvp.Value;

                    var answerEntity = new ResultAnswer
                    {
                        ResultId   = result.ResultId,
                        QuestionId = questionId,
                        OptionId   = optionId
                    };

                    _db.ResultAnswers.Add(answerEntity);
                }

                await _db.SaveChangesAsync(ct);
            }

            // Hent quiz for å kunne sette tittel/fagkode på DTO
            var quiz = await _db.Quizzes.AsNoTracking()
                .FirstOrDefaultAsync(q => q.QuizId == quizId, ct);

            var read = new ResultReadDto(
                ResultId:       result.ResultId,
                UserId:         result.UserId,
                QuizId:         result.QuizId,
                QuizTitle:      quiz?.Title ?? string.Empty,
                SubjectCode:    quiz?.SubjectCode ?? string.Empty,
                CorrectCount:   result.CorrectCount,
                TotalQuestions: result.TotalQuestions,
                CompletedAt:    result.CompletedAt,
                Percentage:     result.Percentage
            );

            return CreatedAtAction(
                nameof(GetResult),
                new { quizId, resultId = result.ResultId },
                read
            );
        }

        [HttpGet("{quizId:int}/results/{resultId:int}")]
        [Authorize]
        public async Task<ActionResult<ResultReadDto>> GetResult(
            int quizId,
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
                .FirstOrDefaultAsync(x => x.res.ResultId == resultId && x.res.QuizId == quizId, ct);

            if (r is null) return NotFound();

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

        // GET /api/quiz/my/results
        // All results for the current logged-in user.
        [HttpGet("my/results")]
        [Authorize]
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
