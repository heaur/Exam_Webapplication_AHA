using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuizApp.Models
{
    public class Question
    {
        [Key]                                // PK
        public int QuestionId { get; set; }

        [Required]                           // Må høre til quiz
        public int QuizId { get; set; }

        [Required]                           // tekst kan ikke være tom
        [MaxLength(500)]                     // Maks 500 tegn
        public string Text { get; set; } = string.Empty; //Default value

        public int? AnswerOptionID { get; set; }

        public Quiz? Quiz { get; set; } //makes nullable

        //Samling initialiserer for å unngå null referanser
        public List<Option> Options { get; set; } = new(); //initialiserer en tom liste
    }
}