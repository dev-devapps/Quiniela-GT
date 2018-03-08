using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DemoQuiniela.Controllers
{
    public class ListadoController : Controller
    {
        public ActionResult Index()
        {
            return View ();
        }

        public ActionResult Test()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
