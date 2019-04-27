﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Practices.EnterpriseLibrary.Caching;
using Microsoft.Practices.EnterpriseLibrary.Caching.Expirations;

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

                questions.ElapseTime = quiz.ElapseTime;

            }
            else
            {
                var quizList = (List<QuestionOptions>)Session["Questions"];

                questions = quizList.Where(x => x.IsAnswered == false).FirstOrDefault();
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

            var quizList = (List<QuestionOptions>)Session["Questions"];
            int index = quizList.FindIndex(a => a.Id == options.Id);
            var quiz = quizList[index];
            quiz.IsAnswered = true;
            quizList[index] = quiz;
            Session["Questions"] = quizList;



            return RedirectToAction("Index", new { isFromPost = true });
        }

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


        private Quiz GetQuizRecords()
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

        public int Score { get; set; }
    }

}