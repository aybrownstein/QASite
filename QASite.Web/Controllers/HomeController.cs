using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QASite.Web.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using QASite.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace QASite.Web.Controllers
{
    public class HomeController : Controller
    {
        private string _connectionString;

        public HomeController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ConStr");
        }


        public IActionResult Index()
        {
            var repo = new QARepository(_connectionString);
            List<Question> questions = repo.GetQuestions();
            var vm = new HomeViewModel { Questions = questions };
            return View(vm);
        }
        [Authorize]
    public IActionResult AskQuestion()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public IActionResult SubmitQuestion(Question question, List<string> tags)
        {
            question.DatePosted = DateTime.Now;
            var repo = new QARepository(_connectionString);
            var user = repo.GetByEmail(User.Identity.Name);
            question.UserId = user.Id;
            repo.AddQuestion(question, tags);
            return Redirect("/");
        }

        public IActionResult ViewQuestion(int id)
        {
            var repo = new QARepository(_connectionString);
            var question = repo.Get(id);
            var vm = new ViewQuestionModel { Question = question };
            if (User.Identity.IsAuthenticated)
            {
                vm.CurrentUser = repo.GetByEmail(User.Identity.Name);
            }
            if (question == null)
            {
                return Redirect("/");
            }
            return View(vm);
        }

        public IActionResult Login(string returnUrl)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public IActionResult Login(string email, string password, string returnUrl)
        {
            var repo = new QARepository(_connectionString);
            var user = repo.Login(email, password);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var claims = new List<Claim>
            {
                new Claim("user", "email")
            };

            HttpContext.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies", "user", "role"))).Wait();
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return Redirect("/");
        }

        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Signup(User user, string password)
        {
            var repo = new QARepository(_connectionString);
            repo.AddUser(user, password);
            return Redirect("/");
        }

        [HttpPost]
        [Authorize]
        public IActionResult AddAnswer(int questionId, string text)
        {
            var repo = new QARepository(_connectionString);
            var user = repo.GetByEmail(User.Identity.Name);
            var answer = new Answer
            {
                Text = text,
                QuestionId = questionId,
                UserId = user.Id
            };
            repo.AddAnswer(answer);
            return RedirectToAction("ViewQuestion", new { id = questionId });
        }

        [HttpPost]
        [Authorize]
        public void AddQuestionLike(int questionId)
        {
            var repo = new QARepository(_connectionString);
            var user = repo.GetByEmail(User.Identity.Name);
            repo.AddQuestionLike(questionId, user.Id);
        }

        public IActionResult GetLikes(int questionId)
        {
            var repo = new QARepository(_connectionString);
            return Json(new { likes = repo.GetQuestionLikes(questionId });
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync().Wait();
            return Redirect("/");
        }
    }
}
