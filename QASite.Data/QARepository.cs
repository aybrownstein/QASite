using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QASite.Data
{
    public class QARepository
    {
        private readonly string _connectionString;

        public QARepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Question> GetQuestions()
        {
           using var context = new QADbContext(_connectionString);
            return context.Questions.Include(q => q.Answers)
                .Include(q => q.Likes)
                .Include(q => q.QuesionsTags)
                .ThenInclude(q => q.Tag)
                .OrderByDescending(q => q.DatePosted)
                .ToList();
           }

        public Question Get(int id)
        {
            using var context = new QADbContext(_connectionString);
            return context.Questions.Include(q => q.User)
                .ThenInclude(q => q.LikedQuestions)
                .Include(q => q.Answers)
                .ThenInclude(a => a.User)
                .Include(q => q.Likes)
                .Include(u => u.QuesionsTags)
                .ThenInclude(qt => qt.Tag)
                .FirstOrDefault(q => q.Id == id);
        }

        public void AddQuestion(Question question, List<string> tags)
        {
           using var context = new QADbContext(_connectionString);
            context.Questions.Add(question);
            context.SaveChanges();

            foreach(string tag in tags)
            {
                Tag t = GetTag(tag);
                int tagId;
                if(t == null)
                {
                    tagId = AddTag(tag);
                }
                else
                {
                    tagId = t.Id;
                }
                context.QuesionsTags.Add(new QuesionsTags
                {
                    QuestionId = question.Id,
                    TagId = tagId
                });
            }
            context.SaveChanges();
        }

        private Tag GetTag(string name)
        {
            using var context = new QADbContext(_connectionString);
            return context.Tags.FirstOrDefault(t => t.Name == name);
        }

        private int AddTag(string name)
        {
           using var context = new QADbContext(_connectionString);
            var tag = new Tag { Name = name };
            context.Tags.Add(tag);
            context.SaveChanges();
            return tag.Id;
            
        }

        public void AddAnswer(Answer answer)
        {
            using var context = new QADbContext(_connectionString);
            context.Answers.Add(answer);
            context.SaveChanges();
        }

        public void AddQuestionLike(int questionId, int userId)
        {
            using var context = new QADbContext(_connectionString);
            var like = new QuestionLike
            {
                QuestionId = questionId,
                UserId = userId
            };
            context.QuestionLikes.Add(like);
            context.SaveChanges();
        }

        public int GetQuestionLikes(int questionId)
        {
            using var context = new QADbContext(_connectionString);
            return context.QuestionLikes.Count(q => q.QuestionId == questionId);
        }

        public void AddUser(User user, string password)
        {
            user.PasswordHash = PasswordHelper.HashPassword(password);

            using var context = new QADbContext(_connectionString);
            context.Users.Add(user);
            context.SaveChanges();
        }

        public User Login(string email, string password)
        {
            var user = GetByEmail(email);
            if (user == null)
            {
                return null;
            }
            bool isCorrectPassword = PasswordHelper.PasswordMatch(password, user.PasswordHash);
            if (isCorrectPassword)
            {
                return user;
            }

            return null;
        }

        public User GetByEmail(string email)
        {
            using var context = new QADbContext(_connectionString);
            return context.Users.FirstOrDefault(u => u.Email == email);
        }
    }
}
