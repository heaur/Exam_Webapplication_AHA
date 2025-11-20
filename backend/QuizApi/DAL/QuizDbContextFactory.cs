using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using QuizApi.DAL;

public class QuizDbContextFactory : IDesignTimeDbContextFactory<QuizDbContext>
{
    public QuizDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<QuizDbContext>();
        optionsBuilder.UseSqlite("Data Source=quiz.db");
        return new QuizDbContext(optionsBuilder.Options);
    }
}