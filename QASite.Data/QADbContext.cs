using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace QASite.Data
{
    public class QADbContext: DbContext
    {
        private readonly string _connectionString;

        public QADbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Answer> Answers { get; set; }

        public DbSet<Question> Questions { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<QuesionsTags> QuesionsTags { get; set; }
        public DbSet<QuestionLike> QuestionLikes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
                {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            modelBuilder.Entity<QuesionsTags>()
                .HasKey(qt => new { qt.QuestionId, qt.TagId });

            modelBuilder.Entity<QuesionsTags>()
                .HasOne(qt => qt.Question)
                .WithMany(q => q.QuesionsTags)
                .HasForeignKey(q => q.QuestionId);

            modelBuilder.Entity<QuesionsTags>()
                .HasOne(qt => qt.Tag)
                .WithMany(t => t.QuesionsTags)
                .HasForeignKey(t => t.TagId);

            modelBuilder.Entity<QuestionLike>()
                .HasKey(qt => new { qt.QuestionId, qt.UserId });

            modelBuilder.Entity<QuestionLike>()
                .HasOne(qt => qt.Question)
                .WithMany(q => q.Likes)
                .HasForeignKey(q => q.QuestionId);

            modelBuilder.Entity<QuestionLike>()
                .HasOne(qt => qt.User)
                .WithMany(t => t.LikedQuestions)
                .HasForeignKey(q => q.UserId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
