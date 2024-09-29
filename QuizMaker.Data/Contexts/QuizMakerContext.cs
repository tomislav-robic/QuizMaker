using QuizMaker.Core.Entities;
using System.Data.Entity;

namespace QuizMaker.Data.Contexts
{
    public class QuizMakerContext : DbContext
    {
        public QuizMakerContext() : base("name=QuizMakerDb")
        {
        }

        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<QuizTag> QuizTags { get; set; }
        public DbSet<QuestionTag> QuestionTags { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Konfiguracije za entitet Quiz
            modelBuilder.Entity<Quiz>().ToTable("Quizzes")
                .HasKey(q => q.Id)
                .Property(q => q.Name)
                .IsRequired()
                .HasMaxLength(255);
            modelBuilder.Entity<Quiz>()
                .HasIndex(q => q.Name)
                .IsUnique()
                .HasName("IX_Quiz_Name");
            modelBuilder.Entity<Quiz>()
                .HasIndex(q => q.EditedAt)
                .HasName("IX_Quiz_EditedAt");

            // Konfiguracije za entitet Question
            modelBuilder.Entity<Question>()
                .HasKey(q => q.Id)
                .Property(q => q.Text)
                .IsRequired()
                .HasMaxLength(1000);
            modelBuilder.Entity<Question>()
                .HasIndex(q => q.Text)
                .IsUnique()
                .HasName("IX_Question_Text");
            modelBuilder.Entity<Question>()
                .HasIndex(q => q.EditedAt)
                .HasName("IX_Question_EditedAt");

            // Konfiguracije za entitet Tag
            modelBuilder.Entity<Tag>()
                .HasKey(t => t.Id)
                .Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(255);
            modelBuilder.Entity<Tag>()
                .HasIndex(t => t.Name)
                .IsUnique()
                .HasName("IX_Tag_Name");  

            // Konfiguracije za mnogostruke veze - QuizQuestion
            modelBuilder.Entity<QuizQuestion>()
                .HasKey(qq => new { qq.QuizId, qq.QuestionId });
            modelBuilder.Entity<QuizQuestion>()
                .HasRequired(qq => qq.Quiz)
                .WithMany(q => q.QuizQuestions)
                .HasForeignKey(qq => qq.QuizId);
            modelBuilder.Entity<QuizQuestion>()
                .HasRequired(qq => qq.Question)
                .WithMany(q => q.QuizQuestions)
                .HasForeignKey(qq => qq.QuestionId);

            // Konfiguracije za mnogostruke veze - QuizTag
            modelBuilder.Entity<QuizTag>()
                .HasKey(qt => new { qt.QuizId, qt.TagId });
            modelBuilder.Entity<QuizTag>()
                .HasRequired(qt => qt.Quiz)
                .WithMany(q => q.QuizTags)
                .HasForeignKey(qt => qt.QuizId);
            modelBuilder.Entity<QuizTag>()
                .HasRequired(qt => qt.Tag)
                .WithMany(t => t.QuizTags)
                .HasForeignKey(qt => qt.TagId);

            // Konfiguracije za mnogostruke veze - QuestionTag
            modelBuilder.Entity<QuestionTag>()
                .HasKey(qt => new { qt.QuestionId, qt.TagId });
            modelBuilder.Entity<QuestionTag>()
                .HasRequired(qt => qt.Question)
                .WithMany(q => q.TagQuestions)
                .HasForeignKey(qt => qt.QuestionId);
            modelBuilder.Entity<QuestionTag>()
                .HasRequired(qt => qt.Tag)
                .WithMany(t => t.TagQuestions)
                .HasForeignKey(qt => qt.TagId);
        }
    }
}
