using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuizApp.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; } //PK

        [Required]
        [MaxLength(25)]
        public string Username { get; set; } = string.Empty; //default value

        [Required]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty; //default value

        //Lists all quizzes created by the user
        public List<Quiz> Creations { get; set; } = new();

        //Lists all quiz results taken by the user
        public List<Result> History { get; set; } = new();
    }
}