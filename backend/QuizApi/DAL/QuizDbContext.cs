using Microsoft.EntityFrameworkCore;
using QuizApi.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace QuizApi.DAL
{
    public class QuizDbContext : IdentityDbContext
    {
        public QuizDbContext(DbContextOptions<QuizDbContext> options) : base(options) { }

        // DbSets for each entity
        public DbSet<Quiz> Quizzes { get; set; } = default!;
        public DbSet<Question> Questions { get; set; } = default!;
        public DbSet<Option> Options { get; set; } = default!;
        public DbSet<Result> Results { get; set; } = default!;
        public DbSet<User> Users { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Quiz
            modelBuilder.Entity<Quiz>(entity =>
            {
                entity.HasKey(q => q.QuizId);

                entity.Property(q => q.Title)
                      .IsRequired()
                      .HasMaxLength(55);

                entity.Property(q => q.Description)
                      .HasMaxLength(255);

                // Quiz (many) -> (1) User (Owner)
                entity.HasOne(q => q.Owner)
                      .WithMany(u => u.Creations)
                      .HasForeignKey(q => q.OwnerId)
                      .OnDelete(DeleteBehavior.Restrict); 
            });

            // Question
            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasKey(q => q.QuestionId);

                entity.Property(q => q.Text)
                      .IsRequired()
                      .HasMaxLength(500);

                // Question (many) -> (1) Quiz
                entity.HasOne(q => q.Quiz)
                      .WithMany(z => z.Questions)
                      .HasForeignKey(q => q.QuizId)
                      .OnDelete(DeleteBehavior.Cascade);

                
            });

            // Option
            modelBuilder.Entity<Option>(entity =>
            {
                entity.HasKey(o => o.OptionID);

                entity.Property(o => o.Text)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(o => o.IsCorrect)
                      .IsRequired();

                // Option (many) -> (1) Question
                entity.HasOne(o => o.Question)
                      .WithMany(q => q.Options)
                      .HasForeignKey(o => o.QuestionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Result
            modelBuilder.Entity<Result>(entity =>
            {
                entity.HasKey(r => r.ResultId);

                entity.Property(r => r.CorrectCount)
                      .IsRequired();

                entity.Property(r => r.TotalQuestions)
                      .IsRequired();

                entity.Property(r => r.CompletedAt)
                      .IsRequired();

                // Result (many) -> (1) User
                entity.HasOne(r => r.User)
                      .WithMany(u => u.History)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Result (many) -> (1) Quiz
                entity.HasOne(r => r.Quiz)
                      .WithMany() 
                      .HasForeignKey(r => r.QuizId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId);

                entity.Property(u => u.Username)
                      .IsRequired()
                      .HasMaxLength(25);

                entity.Property(u => u.Password)
                      .IsRequired()
                      .HasMaxLength(255);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}

