using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizApi.Domain
{
    public class Result
    {
        [Key] 
        public int ResultId { get; set; }    // PK


        [Required]
        public int UserId { get; set; }      // FK to User

        [Required]
        public int QuizId { get; set; }      // FK to Quiz


        // Number of correct answers
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Antall riktige kan ikke være negativt.")]
        public int CorrectCount { get; set; }

        // Total number of questions in the quiz
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Total spørsmål må være minst 1.")]
        public int TotalQuestions { get; set; }

        // Timestamp for when the quiz was completed
        [Required]
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User? User { get; set; }
        public Quiz? Quiz { get; set; }

        // Computed property for percentage score
        [NotMapped]
        public double Percentage => TotalQuestions > 0
            ? (double)CorrectCount / TotalQuestions * 100.0
            : 0.0;
    }
}