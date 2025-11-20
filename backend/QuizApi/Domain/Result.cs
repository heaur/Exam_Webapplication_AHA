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
        public string UserId { get; set; } = string.Empty;      // FK to User (Identity key)

        [Required]
        public int QuizId { get; set; }      // FK to Quiz


        // Number of correct answers
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Correct answers must be non-negative.")]
        public int CorrectCount { get; set; }

        // Total number of questions in the quiz
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Total questions must be at least 1.")]
        public int TotalQuestions { get; set; }

        // Timestamp for when the quiz was completed
        [Required]
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ApplicationUser? User { get; set; }
        public Quiz? Quiz { get; set; }

        // Computed property for percentage score
        [NotMapped]
        public double Percentage => TotalQuestions > 0
            ? (double)CorrectCount / TotalQuestions * 100.0
            : 0.0;
    }
}
