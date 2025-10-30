using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuizApp.Models
{
    public class Option
    {
        //pk 
        [Key]
        public int OptionID { get; set; }

        //FK
        [Required]
        public int QuestionId { get; set; }

        //text for the alternatives 
        [Required]
        [MaxLength(200)]
        public string Text { get; set; } = string.Empty;

        //if the alternativ is corect 
        [Required]
        public bool IsCorrect { get; set; }

        public Question? Question{ get; set; } //makes nullable
    }
    
}