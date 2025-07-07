using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using PlanPro.Models;

namespace PlanPro.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private Entities db = new Entities(); 

        public ActionResult Index()
        {
            var messages = db.Messages
                             .Where(m => m.Category == "Motivational" || m.Category == "Health")
                             .OrderBy(m => m.Id)
                             .ToList();

            return View(messages);
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
    }
}
