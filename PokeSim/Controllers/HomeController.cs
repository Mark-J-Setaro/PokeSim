using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PokeSim.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string message = null)
        {
            ViewBag.Message = message;
            return View();
        }

        public ActionResult About(string message = null)
        {
            ViewBag.Message = message;
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Having a problem? Send us a message.";
            return View();
        }

        [HttpPost]
        public ActionResult Contact(string userMessage)
        {
            //todo: save userMessage
            ViewBag.Message = "Thanks, we got your message.";

            return View();
        }

        
    }
}