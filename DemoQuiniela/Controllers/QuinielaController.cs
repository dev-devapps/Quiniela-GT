using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcQuiniela.Models;
using System.Data.SqlClient;
using MvcQuiniela.ViewModel;

namespace DemoQuiniela.Controllers
{
    public class QuinielaController : Controller
    {
        private QuinielaDBContext db = new QuinielaDBContext();
        private GoogleLogin googleParameters = new GoogleLogin();
        private GoogleUserOutputData userLogin = new GoogleUserOutputData();
        private User DatosLogin = new User();
        private string querys;

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SignInGooglePlus()
        {
            var Googleurl = "https://accounts.google.com/o/oauth2/auth?response_type=code&redirect_uri=" + googleParameters.googleplus_redirect_url + "&scope=https://www.googleapis.com/auth/userinfo.email%20https://www.googleapis.com/auth/userinfo.profile&client_id=" + googleParameters.googleplus_client_id + "&prompt=select_account";
            return Redirect(Googleurl);
        }

        public ActionResult Login()
        {
            var url = Request.Url.Query;
            int id_user = 0;

            List<AliasUsuario> aliasDB = new List<AliasUsuario>();
            List<Usuario> userDB = new List<Usuario>();

            if (url != "")
            {
                userLogin = googleParameters.ObtenerCorreo(true, url);

                if (userLogin.email != null)
                {
                    querys = "SELECT *"
                         + "FROM Usuario "
                         + "WHERE us_correoElectronico=@email";

                    userDB = db.Usuarios.SqlQuery(querys, new SqlParameter("@email", userLogin.email)).ToList();

                    if (userDB.Count == 1)
                    {
                        id_user = userDB.ElementAt(0).us_id;

                        DatosLogin.email = userLogin.email;
                        DatosLogin.picture = userLogin.picture;
                        DatosLogin.login = true;

                        if (id_user > 0)
                        {
                            querys = "SELECT *"
                            + "FROM AliasUsuario "
                            + "WHERE al_idUsuario=@iduser "
                            + "AND  al_codigoDeposito is not null";

                            aliasDB = db.AliasUsuario.SqlQuery(querys, new SqlParameter("@iduser", id_user)).ToList();
                        }
                    }

                    ViewBag.DatosLogin = DatosLogin;

                    return View(aliasDB);
                }
                else
                {
                    return HttpNotFound();
                }
                
            }
            else
            {
                return HttpNotFound();
            }

        }

        public ActionResult Posiciones(int id)
        {
            DatosLogin = (User)TempData["DatosLogin"];

            if (DatosLogin != null && DatosLogin.login)
            {
                QuinielaViewModel vm = new QuinielaViewModel();

                querys = "SELECT *"
                        + "FROM AliasUsuario "
                        + "WHERE al_id=@idalias ";

                AliasUsuario aliasSeleccionado = db.Database.SqlQuery<AliasUsuario>(querys, new SqlParameter("@idalias", id)).FirstOrDefault();

                if (aliasSeleccionado != null)
                {
                    DatosLogin.id_alias = id;
                    DatosLogin.nickname = aliasSeleccionado.al_nickname;

                    querys = "SELECT *"
                             + "FROM AliasUsuario "
                             + "WHERE al_idUsuario=@iduser "
                             + "AND  al_codigoDeposito is not null";

                    vm.vm_alias = db.Database.SqlQuery<AliasUsuario>(querys, new SqlParameter("@iduser", aliasSeleccionado.al_idUsuario)).ToList();

                    querys = "SELECT id_alias=al_id, alias=al_nickname, puntos= 0 "
                                 + "FROM AliasUsuario "
                                 + "WHERE al_codigoDeposito is not null";

                    List<TablaPosiciones> tablaPosiciones = db.Database.SqlQuery<TablaPosiciones>(querys).ToList<TablaPosiciones>();

                    if (tablaPosiciones != null && tablaPosiciones.Count > 0)
                    {
                        vm.vm_tablaPosiciones = tablaPosiciones;

                        querys = "SELECT * "
                               + "FROM Partido "
                               + "WHERE pa_estado = 'T'";

                        List<Partido> PartidosJugados = db.Database.SqlQuery<Partido>(querys).ToList<Partido>();

                        if (PartidosJugados != null && PartidosJugados.Count > 0)
                        {
                            foreach (Partido partidoJugado in PartidosJugados)
                            {
                                foreach (TablaPosiciones itemTabla in tablaPosiciones)
                                {
                                    querys = "SELECT *"
                                     + "FROM Marcador "
                                     + "WHERE ma_idAlias = @idAlias "
                                     + "AND  ma_idPartido = @idPartido ";

                                    Marcador pronosticoAlias = db.Database.SqlQuery<Marcador>(querys, new SqlParameter("@idAlias", itemTabla.id_alias), new SqlParameter("@idPartido", partidoJugado.pa_id)).FirstOrDefault();

                                    if (pronosticoAlias != null)
                                        itemTabla.puntos = itemTabla.puntos + itemTabla.CalcularPuntos(pronosticoAlias, partidoJugado);
                                }
                            }
                        }
                    }

                    List<TablaPosiciones> tablaPosicionesOrdenada = (from s in tablaPosiciones
                                                                     orderby s.puntos descending
                                                                     select s).ToList();

                    vm.vm_tablaPosiciones = tablaPosicionesOrdenada;

                    ViewBag.DatosLogin = DatosLogin;

                    return View(vm);
                }
                else
                {
                    return HttpNotFound();
                }
            }
            else
            {
                return HttpNotFound();
            }
        }

        public ActionResult IngresoMarcadores()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult IngresoPronostico()
        {
            ViewBag.DatosLogin = TempData["DatosLogin"];


            return View(db.Equipos.ToList());
        }
    }
}
