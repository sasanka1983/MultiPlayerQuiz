using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Adapter.Controllers
{
    public class HomeController : Controller
    {
        private static string CONNECTION_STRING = ConfigurationManager.ConnectionStrings["Adapters"].ToString();
        public ActionResult Index(string message = "")
        {
            ViewBag.ErrorMessage = string.Empty;
            if(!string.IsNullOrEmpty(message))
            {
                ViewBag.ErrorMessage = message;
            }
            return View();
        }

        [HttpPost]
        public ActionResult Index(User user)
        {
            user.Id = GetUserId(user.Email, user.Password);


            if (user.Id != Guid.Empty)
            {
                Session["user"] = user;

                return RedirectToAction("Index", "Quiz");
            }
            return RedirectToAction("Index", new { message = "Credentials are incorrect" });
            // return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        private Guid GetUserId(string username, string password)
        {

            Guid userGuid = new Guid();

            SqlConnection connection = new SqlConnection(CONNECTION_STRING);
            SqlCommand command = new SqlCommand(
          "SELECT * FROM [User] Where Email='" + username + "' And Password='" + password + "'",
          connection);
            connection.Open();

            SqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    userGuid = new Guid(Convert.ToString(reader["Id"]));
                }
            }
            return userGuid;
        }
    }
}