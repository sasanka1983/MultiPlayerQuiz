using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Adapter.Controllers
{
    public class QuizController : Controller
    {

        private static string CONNECTION_STRING = ConfigurationManager.ConnectionStrings["Adapters"].ToString();

        // GET: Quiz
        public ActionResult Index()
        {
            var quiz = GetQuizRecords();

            var questions = GetQuizQuestion(quiz.Id);

            return View(questions);
        }

        private static QuestionOptions GetQuizQuestion(Guid quizId)
        {
            QuestionOptions quiz = new QuestionOptions();

            SqlConnection connection = new SqlConnection(CONNECTION_STRING);
            var date = DateTime.Now.ToString("dd/MM/yyyy hh:mm");

            SqlCommand command = new SqlCommand(
          @"select question.Id,question.Description,questionoptions.Description as OptionDescription,IsAnswer from question 
inner join quizquestions  on question.id = quizquestions.QuestionId
INNER JOIN questionoptions on questionoptions.QuestionId = quizquestions.QuestionId  WHERE QuizId='" + quizId + "' AND QuizQuestions.IsPublished=0",
          connection);
            connection.Open();

            SqlDataReader reader = command.ExecuteReader();

            List<Options> options = new List<Options>();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    quiz.Id = new Guid(Convert.ToString(reader["Id"]));
                    quiz.Description = Convert.ToString(reader["Description"]);

                    options.Add(new Options { Description = Convert.ToString(reader["OptionDescription"]), IsAnswer = Convert.ToBoolean(reader["IsAnswer"]) });
                }
            }
            else
            {
                Console.WriteLine("No rows found.");
            }
            reader.Close();

            if (quiz.Id != Guid.Empty)
            {
                //  UpdateQuestionPublish(quiz.Id, quizId);
            }

            quiz.OptionsList = options;

            return quiz;
        }

        private static int UpdateQuestionPublish(Guid quizQuestionId, Guid quizId)
        {
            SqlConnection connection = new SqlConnection(CONNECTION_STRING);
            var date = DateTime.Now.ToString("dd/MM/yyyy hh:mm");

            SqlCommand command = new SqlCommand(
          @"Update QuizQuestions SET IsPublished=1 WHERE QuestionId='" + quizQuestionId + "' AND QuizId='" + quizId + "'",
          connection);
            connection.Open();

            return command.ExecuteNonQuery();

        }


        private static Quiz GetQuizRecords()
        {
            Quiz quiz = new Quiz();

            SqlConnection connection = new SqlConnection(CONNECTION_STRING);
            SqlCommand command = new SqlCommand(
          "SELECT top 1 * FROM Quiz;",
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
    }

    public class Options
    {
        public string Description { get; set; }
        public bool IsAnswer { get; set; }
    }

    public class User
    {
        public string Email { get; set; }

    }

}