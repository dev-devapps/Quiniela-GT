using MvcQuiniela.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Data.SqlClient;

namespace DemoQuiniela.Controllers
{
    public class SessionCheck : ActionFilterAttribute
    {
        public int Transaccion { get; set; }

        private User DatosLogin = new User();
        private string querys;
        private QuinielaDBContext db = new QuinielaDBContext();
        List<TransaccionRol> transaccionRol = new List<TransaccionRol>();
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpSessionStateBase Session = filterContext.HttpContext.Session;
            if (Session != null && Session["UserInfo"] == null)
                {
                    HttpContext.Current.Response.Redirect("~/Quiniela");
                }
            else
                {

                    DatosLogin = (User)Session["UserInfo"];
                    querys = "SELECT *"
                             + "FROM TransaccionRol "
                             + "WHERE tr_id_transaccion=@id_transaccion "
                             + "AND tr_id_rol=@id_rol ";

                    transaccionRol = db.TransaccionRol.SqlQuery(querys, new SqlParameter("@id_transaccion", Transaccion), new SqlParameter("@id_rol", DatosLogin.id_rol)).ToList();

                    if (transaccionRol.Count == 0)
                        {
                            HttpContext.Current.Response.Redirect("~/Quiniela");
                        }
                }
        }
    }
}