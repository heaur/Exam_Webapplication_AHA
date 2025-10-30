using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizApi.Domain
{
    /// <summary>
    /// Representerer et resultat av en bruker som har tatt en quiz.
    /// Hver rad i denne tabellen lagrer hvilken bruker som tok quizen, 
    /// hvilken quiz det gjaldt, hvor mange spørsmål som ble besvart riktig, 
    /// totalt antall spørsmål, og når quizen ble fullført.
    /// </summary>
    public class Result
    {
        [Key] 
        public int ResultId { get; set; }    // Primærnøkkel for resultatet

        // ------------------- RELASJONER -------------------

        [Required]
        public int UserId { get; set; }      // Fremmednøkkel til brukeren som tok quizen

        [Required]
        public int QuizId { get; set; }      // Fremmednøkkel til hvilken quiz dette resultatet gjelder

        // ------------------- RESULTATDATA -------------------

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Antall riktige kan ikke være negativt.")]
        public int CorrectCount { get; set; }    // Antall riktige svar brukeren hadde

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Total spørsmål må være minst 1.")]
        public int TotalQuestions { get; set; }  // Totalt antall spørsmål i quizen

        [Required]
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

        // Navigasjonsreferanser nullable
        public User? User { get; set; }
        public Quiz? Quiz { get; set; }

        [NotMapped]
        public double Percentage => TotalQuestions > 0
            ? (double)CorrectCount / TotalQuestions * 100.0
            : 0.0;
    }
}