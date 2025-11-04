using Microsoft.AspNetCore.Mvc;

namespace QuizApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly ILogger<QuizController> _logger;

        // Constructor for dependency injection of the logger
        public QuizController(ILogger<QuizController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                _logger.LogInformation("Fetching all quizzes");

                var quizzes = new[]
                {
                    new { Id = 1, Title = "Sample Quiz" }
                };

                return Ok(quizzes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch quizzes");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}
