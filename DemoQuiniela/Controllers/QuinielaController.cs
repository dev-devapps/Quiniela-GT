using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcQuiniela.Models;

namespace DemoQuiniela.Controllers
{
    public class QuinielaController : Controller
    {
        private QuinielaDBContext db = new QuinielaDBContext();

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

        public ActionResult IngresoPronostico()
        {
            ViewBag.Message = "Your contact page.";

            return View(db.Equipos.ToList());
        }
    }
}
