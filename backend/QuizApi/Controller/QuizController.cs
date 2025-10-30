using Microsoft.AspNetCore.Mvc;

namespace QuizApi.Controllers


//Dette er en test for å teste om frontend og backend fungerer sammen
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAll()
        {
            var quizzes = new[]
            {
                new { Id = 1, Title = "C# Basics", QuestionCount = 5 },
                new { Id = 2, Title = "ASP.NET Fundamentals", QuestionCount = 10 }
            };

            return Ok(quizzes);
        }
    }
}
// Her slutter testen