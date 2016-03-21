using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebContacts.Controllers
{
    public class HomeController : Controller
    {
        private string user1 = "Antoine Coulombe";
        private string user2 = "Samuel Bonneville";
        private string userStatus = "Étudiant";
        private string userDept = "Informatique de gestion";
        private string userSchool = "Collège Lionel-Groulx";

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.SubTitle = "À propos";
            ViewBag.Message = "Gestion de films et d'acteurs";
            ViewBag.Description = @"Ce site de gestion de films et d'acteurs représente un solutionnaire de
    travail pratique dans le cadre de développement d'application Web client serveur.
    Il existe simplement dans un but pédagogique et ne représente aucunement la réalité.";

            ViewBag.User1_name = user1;
            ViewBag.User2_name = user2;
            ViewBag.User_status = userStatus;
            ViewBag.User_dept = userDept;
            ViewBag.User_school = userSchool;

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.User1 = user1;
            ViewBag.User2 = user2;
            ViewBag.User1_email = "antoine_coulombe@BDFI.com";
            ViewBag.User2_email = "samuel_bonneville@BDFI.com";
            ViewBag.User_status = userStatus;
            ViewBag.User_dept = userDept;
            ViewBag.User_school = userSchool;

            return View();
        }
    }
}