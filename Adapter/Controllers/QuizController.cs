using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Mvc;

namespace Adapter.Controllers
{
    public class QuizController : Controller
    {

        private static string CONNECTION_STRING = ConfigurationManager.ConnectionStrings["Adapters"].ToString();

        // GET: Quiz
        public ActionResult Index(bool isFromPost = false)
        {
            QuestionOptions questions = new QuestionOptions();
            if (!isFromPost)
            {
                var quiz = GetQuizRecords();

                questions = GetQuizQuestion(quiz.Id);

                var user = (User)Session["user"];
                user.QuizId = quiz.Id;
                Session["user"] = user;


                questions.ElapseTime = quiz.ElapseTime;
                ViewBag.FromPost = false;

            }
            else
            {
                var quizList = (List<QuestionOptions>)Session["Questions"];

                //ObjectCache cache = MemoryCache.Default;
                //var quizList = (List<QuestionOptions>)cache.Get("Questions");

                questions = quizList.Where(x => x.IsAnswered == false).FirstOrDefault();
                ViewBag.FromPost = true;

            }

            ViewBag.IsCompletedQuestions = false;

            if (questions == null)
            {
                var user = (User)Session["user"];

                UpdateScores(user.Id, user.Score, user.QuizId);

                ViewBag.Score = user.Score;
                ViewBag.UserName = user.Email;

                ViewBag.IsCompletedQuestions = true;
            }


            return View(questions);
        }

        [HttpPost]
        public ActionResult Index(QuestionOptions options, bool chkCorrectAnswer)
        {
            if (chkCorrectAnswer)
            {
                var user = (User)Session["user"];
                user.Score = user.Score + 5;
                Session["user"] = user;
            }

            //ObjectCache cache = MemoryCache.Default;
            //var quizList = (List<QuestionOptions>)cache.Get("Questions");
            var quizList = (List<QuestionOptions>)Session["Questions"];
            int index = quizList.FindIndex(a => a.Id == options.Id);
            var quiz = quizList[index];
            quiz.IsAnswered = true;
            quizList[index] = quiz;
            Session["Questions"] = quizList;

            return RedirectToAction("Index", new { isFromPost = true });
        }


        //public ActionResult List()
        //{

        //}



        private QuestionOptions GetQuizQuestion(Guid quizId)
        {
            List<QuestionOptions> quizList = new List<QuestionOptions>();
            QuestionOptions quiz = new QuestionOptions();
            SqlConnection connection = new SqlConnection(CONNECTION_STRING);
            var date = DateTime.Now.ToString("dd/MM/yyyy hh:mm");

            SqlCommand command = new SqlCommand(
          @"select question.Id,question.Description,questionoptions.Description as OptionDescription,IsAnswer from question 
inner join quizquestions  on question.id = quizquestions.QuestionId
INNER JOIN questionoptions on questionoptions.QuestionId = quizquestions.QuestionId  WHERE QuizId='" + quizId + "' AND QuizQuestions.IsPublished=0" +
"Order by question.Id",
          connection);
            connection.Open();

            SqlDataReader reader = command.ExecuteReader();

            //Guid prevId = Guid.Empty;
            //List<Options> options = new List<Options>();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    var guid = new Guid(Convert.ToString(reader["Id"]));

                    if (quizList.Where(x => x.Id == guid).FirstOrDefault() == null)
                    {
                        quiz = new QuestionOptions();
                    }

                    //if (prevId != guid)
                    //{
                    //    if (prevId != Guid.Empty)
                    //    {
                    //        quizList.Add(quiz);
                    //    }
                    //    quiz = new QuestionOptions();
                    //}
                    //prevId = guid;
                    quiz.Id = guid;
                    quiz.Description = Convert.ToString(reader["Description"]);
                    if (quiz.OptionsList == null)
                    {
                        quiz.OptionsList = new List<Options>();
                    }
                    quiz.OptionsList.Add(new Options { Description = Convert.ToString(reader["OptionDescription"]), IsAnswer = Convert.ToBoolean(reader["IsAnswer"]) });

                    if (quizList.Where(x => x.Id == guid).FirstOrDefault() == null)
                    {
                        quizList.Add(quiz);
                    }

                }
            }

            reader.Close();

            if (quiz.Id != Guid.Empty)
            {
                //  UpdateQuestionPublish(quiz.Id, quizId);
            }

            // quiz.OptionsList = options;
            Session["Questions"] = quizList;
            //ObjectCache cache = MemoryCache.Default;
            // Store data in the cache    
            //CacheItemPolicy cacheItemPolicy = new CacheItemPolicy();
            //cacheItemPolicy.AbsoluteExpiration = DateTime.Now.AddHours(1.0);
            //cache.Add("Questions", quizList, cacheItemPolicy);
            return quizList.FirstOrDefault();
        }

        private int UpdateQuestionPublish(Guid quizQuestionId, Guid quizId)
        {
            SqlConnection connection = new SqlConnection(CONNECTION_STRING);
            var date = DateTime.Now.ToString("dd/MM/yyyy hh:mm");

            SqlCommand command = new SqlCommand(
          @"Update QuizQuestions SET IsPublished=1 WHERE QuestionId='" + quizQuestionId + "' AND QuizId='" + quizId + "'",
          connection);
            connection.Open();

            return command.ExecuteNonQuery();

        }

        private int UpdateScores(Guid userId, int score, Guid quizId)
        {
            SqlConnection connection = new SqlConnection(CONNECTION_STRING);
            var date = DateTime.Now.ToString("dd/MM/yyyy hh:mm");

            SqlCommand command = new SqlCommand(
          @"Update quiz SET IsQuizCompleted=1 Where id='"+ quizId+"' ; Update score set score =" + score + " where userid='" + userId + "' AND quizId='" + quizId + "'",
          connection);
            connection.Open();

            return command.ExecuteNonQuery();
        }

        private List<Quiz> GetAllQuizRecords()
        {
            List<Quiz> quizList = new List<Quiz>();

            SqlConnection connection = new SqlConnection(CONNECTION_STRING);
            SqlCommand command = new SqlCommand(
          "SELECT * FROM Quiz where IsQuizCompleted=0",
          connection);
            connection.Open();

            SqlDataReader reader = command.ExecuteReader();

            Quiz quiz;

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    quiz = new Quiz();
                    quiz.Id = new Guid(Convert.ToString(reader["Id"]));
                    quiz.Name = Convert.ToString(reader["Name"]);
                    quiz.StartTime = Convert.ToDateTime(reader["StartTime"]);
                    quiz.ElapseTime = Convert.ToInt32(reader["ElapseTime"]);

                    quizList.Add((quiz));
                }
            }
            else
            {
                Console.WriteLine("No rows found.");
            }
            reader.Close();

            return quizList;
        }

        private Quiz GetQuizRecords()
        {
            Quiz quiz = new Quiz();

            SqlConnection connection = new SqlConnection(CONNECTION_STRING);
            SqlCommand command = new SqlCommand(
          "SELECT top 1 * FROM Quiz where IsQuizCompleted=0",
          connection);
            connection.Open();

            SqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    quiz.Id = new Guid(Convert.ToString(reader["Id"]));
                    quiz.Name = Convert.ToString(reader["Name"]);
                    quiz.StartTime = Convert.ToDateTime(reader["StartTime"]);
                    quiz.ElapseTime = Convert.ToInt32(reader["ElapseTime"]);
                }
            }
            else
            {
                Console.WriteLine("No rows found.");
            }
            reader.Close();

            return quiz;
        }
    }

    public class Quiz
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public int ElapseTime { get; set; }
    }

    public class QuestionOptions
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public int ElapseTime { get; set; }
        public List<Options> OptionsList { get; set; }

        public bool IsAnswered { get; set; }


    }

    public class Options
    {
        public string Description { get; set; }
        public bool IsAnswer { get; set; }

    }

    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public Guid QuizId { get; set; }
        public int Score { get; set; }
    }

}