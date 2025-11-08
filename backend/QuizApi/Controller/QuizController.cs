using Microsoft.AspNetCore.Mvc;
using QuizApi.DTOs;
using Microsoft.Extensions.Logging;
using System;

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

        //Metoder for user:
        // Lag bruker/create [AllowAnonymous]
        // Logg inn/login 
        // Hent brukerinfo/getuser [Authorize]

        //Metoder for quiz:
        // Lag quiz/create [Authorize]
        // Hent en quiz/getquiz [AllowAnonymous]
        // List opp alle quizzer/listquizzes
        // Oppdater quiz/updatequiz [Authorize]
        // Slett quiz/deletequiz [Authorize]

        //Metoder for questions:
        // Legg til spørsmål/addquestion
        // Hent spørsmål/getquestion
        // Oppdater spørsmål/updatequestion
        // Slett spørsmål/deletequestion

        // Metoder for Options:
        // Legg til svaralternativ/addoption
        // Hent svaralternativ/getoption
        // Oppdater svaralternativ/updateoption
        // Slett svaralternativ/deleteoption

        // Metoder for Result:
        // Start forsøk/startattempt [Authorize]
        // Send inn svar/submitanswers [Authorize]
        // Hent resultat/getresult [Authorize]
    }
}
