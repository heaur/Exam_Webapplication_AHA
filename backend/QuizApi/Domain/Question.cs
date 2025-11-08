using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuizApi.Domain
{
    public class Question
    {
        [Key]                      
        public int QuestionId { get; set; } // PK

        [Required]                           
        public int QuizId { get; set; } // FK

        [Required]                           
        [MaxLength(500)]                    
        public string Text { get; set; } = string.Empty; //Default value

        public int? AnswerOptionID { get; set; } // FK to correct option

        public Quiz? Quiz { get; set; } //makes nullable

        public List<Option> Options { get; set; } = new(); // Initialize to empty list
    }
}