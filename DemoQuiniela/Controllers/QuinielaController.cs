using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DemoQuiniela.Controllers
{
    public class QuinielaController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Posiciones()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult IngresoMarcadores()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
