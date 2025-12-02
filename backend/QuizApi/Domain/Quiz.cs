using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuizApi.Domain
{
    /// Domain entity representing a quiz.
    /// This is the class Entity Framework maps to the "Quizzes" table in the database.
    public class Quiz
    {
        /// Primary key for the quiz entity.
        /// EF Core will treat this as the identity column.
        [Key]
        public int QuizId { get; set; }

        // --------------------------------------------------------------------
        // Basic quiz metadata
        // --------------------------------------------------------------------

        /// Human-readable title shown in the UI.
        /// Example: "Web Exam", "Datasikkerhet – Midterm".
        [Required]
        [MaxLength(55)]
        public string Title { get; set; } = string.Empty;

        /// Short description shown under the title in the UI.
        /// Optional field – can be null if the owner does not provide one.
        /// Example: "Practice questions for ITPE3200 exam".
        [MaxLength(255)]
        public string? Description { get; set; }

        /// Course code the quiz belongs to.
        /// Used by the frontend to group quizzes on the homepage by course,
        /// e.g. "ITPE3200", "ITPE3100".
        [Required]
        [MaxLength(20)]
        public string SubjectCode { get; set; } = "OTHER";

        /// Optional URL to a thumbnail / cover image for the quiz card.
        /// The frontend reads this to display an image inside the quiz card.
        [MaxLength(512)]
        public string? ImageUrl { get; set; }

        // --------------------------------------------------------------------
        // Owner / identity
        // --------------------------------------------------------------------

        /// Foreign key to the user who owns/created the quiz.
        /// This is typically the ASP.NET Identity user id.
        /// </summary>
        public string? OwnerId { get; set; }

        /// Navigation property for the owning user.
        /// Enables EF Core to load the ApplicationUser related to this quiz.
        public ApplicationUser? Owner { get; set; }

        // --------------------------------------------------------------------
        // Timestamps
        // --------------------------------------------------------------------

        /// UTC timestamp when the quiz was first created.
        /// Set once on creation; never changed afterwards.
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// UTC timestamp for the last time the quiz metadata was updated.
        /// Null when the quiz has never been edited after creation.
        public DateTime? UpdatedAt { get; set; }

        // --------------------------------------------------------------------
        // Publication state
        // --------------------------------------------------------------------

        /// Indicates whether the quiz is visible for students in the frontend.
        /// When false, the quiz is considered a draft.
        [Required]
        public bool IsPublished { get; set; } = false;

        /// UTC timestamp for when the quiz was published.
        /// Null if the quiz has never been published.
        public DateTime? PublishedAt { get; set; }

        // --------------------------------------------------------------------
        // Relations
        // --------------------------------------------------------------------

        /// Collection of questions that belong to this quiz.
        /// Initialized with an empty list to avoid null-checks in the code.
        public List<Question> Questions { get; set; } = new();
    }
}
