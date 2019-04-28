using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Adapter.Controllers
{
    public class RankingController : Controller
    {
        private static string CONNECTION_STRING = ConfigurationManager.ConnectionStrings["Adapters"].ToString();
        // GET: Ranking
        public ActionResult GetRanking()
        {
           // Guid quizId = new Guid();
            var user = (User)Session["user"];
            List<User> lstUserRanking = new List<User>();
            SqlConnection connection = new SqlConnection(CONNECTION_STRING);

            SqlCommand command = new SqlCommand(
        "select Email, quiz.Name, Score from Score inner join quiz on score.QuizId = Quiz.id inner join [user] on score.UserId =[user].id where quizId ='" + user.QuizId+"'",
        connection);
            connection.Open();

            SqlDataReader reader = command.ExecuteReader();
            User userRanking;
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    userRanking = new User();
                    userRanking.Email = Convert.ToString(reader["Email"]);
                    userRanking.Name = Convert.ToString(reader["Name"]);
                    userRanking.Score = Convert.ToInt32(reader["Score"]);
                    lstUserRanking.Add(userRanking);
                }
            }
            else
            {
                Console.WriteLine("No rows found.");
            }
            reader.Close();

            return View(lstUserRanking);
        }
    }
}