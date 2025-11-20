using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuizApi.Domain
{
    public class Quiz
    {
        [Key]
        public int QuizId { get; set; } // PK

        // Quiz title and description
        [Required]
        [MaxLength(55)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Description { get; set; }

        // Owner of the quiz
        public int? OwnerId { get; set; }   // FK to User
        public ApplicationUser? Owner { get; set; }    

        // Timestamps
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Publication status
        [Required]
        public bool IsPublished { get; set; } = false;

        public DateTime? PublishedAt { get; set; }

        // Relation to Questions
        public List<Question> Questions { get; set; } = new(); // empty list to avoid null
    }
}
