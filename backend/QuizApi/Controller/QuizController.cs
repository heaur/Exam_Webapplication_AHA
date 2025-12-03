using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApi.DAL;
using QuizApi.Domain;
using QuizApi.DTOs;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace QuizApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly QuizDbContext _db;

        public QuizController(QuizDbContext db)
        {
            _db = db;
        }

        // QUIZ ENDPOINTS
    

        // POST /api/quiz
        // Create a new quiz (owner = current user) with optional nested questions/options.
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<QuizReadDto>> CreateQuiz(
            [FromBody] QuizCreateDto dto,
            CancellationToken ct)
        {
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

            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var now = DateTime.UtcNow;

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

            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            _db.Quizzes.Add(quizEntity);
            await _db.SaveChangesAsync(ct); // need QuizId for questions

            // Create questions + options if provided in one transaction
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
                    await _db.SaveChangesAsync(ct); // gets QuestionId

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

            await tx.CommitAsync(ct);

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
        // Full quiz for "take view" (questions + options)
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

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isOwner = !string.IsNullOrEmpty(currentUserId) &&
                          !string.IsNullOrEmpty(quiz.OwnerId) &&
                          quiz.OwnerId == currentUserId;
            if (!quiz.IsPublished && !isOwner)
            {
                return Forbid();
            }

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
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = _db.Quizzes
                .Include(q => q.Questions)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(q => q.Title.Contains(search));

            // Anonymous users only see published quizzes. Owners also see drafts.
            if (string.IsNullOrEmpty(currentUserId))
            {
                query = query.Where(q => q.IsPublished);
            }
            else
            {
                query = query.Where(q => q.IsPublished || q.OwnerId == currentUserId);
            }

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
    }
}
