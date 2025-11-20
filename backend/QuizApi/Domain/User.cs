using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
namespace QuizApi.Domain
{
    public class ApplicationUser : IdentityUser
    {
        //Lists all quizzes created by the user
        public List<Quiz> Creations { get; set; } = new();

        //Lists all quiz results taken by the user
        public List<Result> History { get; set; } = new();

        // Timestamp for when the user was created
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}