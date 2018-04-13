﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
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

        public ActionResult Error()
        {
            DatosLogin = (User)TempData["DatosLogin"];
            ViewBag.DatosLogin = DatosLogin;

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
                        DatosLogin.id_login = id_user;
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
                               + "WHERE pa_estado in('T','I')";

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
            ViewBag.DatosLogin = TempData["DatosLogin"];
            DatosLogin = (User)TempData["DatosLogin"];
            List<AliasUsuario> aliasDB = new List<AliasUsuario>();

            QuinielaViewModel qvm = new QuinielaViewModel();

            querys = "select id_partido=pa_id, id_equipo1=pa_idEquipo1, equipo1=E1.eq_descripcion, marcador1=pa_marcador1, id_equipo2=pa_idEquipo2, equipo2=E2.eq_descripcion, marcador2=pa_marcador2, id_estadio=es_id, estadio=es_nombre, fecha= convert(varchar(10), pa_fecha, 103), hora= convert(varchar(5), pa_hora, 108), estado=pa_estado "
                    + "from Partido, Equipo as E1, Equipo as E2, Estadio "
                    + "where pa_idEquipo1 = E1.eq_id "
                    + "and pa_idEquipo2 = E2.eq_id "
                    + "and pa_idEstadio = es_id "
                    + "order by pa_fecha";

            List<MarcadorFinal> tablaPronosticos = db.Database.SqlQuery<MarcadorFinal>(querys).ToList<MarcadorFinal>();

            qvm.vm_marcadorFinales = tablaPronosticos;
            return View(qvm);

        }

        public ActionResult IngresoPronostico(int id)
        {
            ViewBag.DatosLogin = TempData["DatosLogin"];
            DatosLogin = (User)TempData["DatosLogin"];
            List<AliasUsuario> aliasDB = new List<AliasUsuario>();

            QuinielaViewModel qvm = new QuinielaViewModel();

            querys = "SELECT *"
                     + "FROM AliasUsuario "
                     + "WHERE al_idUsuario=@iduser "
                     + "AND al_id=@idalias";

            aliasDB = db.AliasUsuario.SqlQuery(querys, new SqlParameter("@iduser", DatosLogin.id_login), new SqlParameter("@idalias", id)).ToList();

            if(aliasDB.Count == 0){
                return Redirect("/Quiniela/Error");
            }

            if (DatosLogin != null && DatosLogin.login){
                

                querys = "select id_partido=pa_id, id_alias= 0, id_equipo1=pa_idEquipo1, equipo1=E1.eq_descripcion, marcador1=pa_marcador1, pronostico1=-1, id_equipo2=pa_idEquipo2, equipo2=E2.eq_descripcion, marcador2=pa_marcador2, pronostico2=-1, puntos=0, id_estadio=es_id, estadio=es_nombre, fecha= convert(varchar(10), pa_fecha, 103), hora= convert(varchar(5), pa_hora, 108), estado=pa_estado "
                        +"from Partido, Equipo as E1, Equipo as E2, Estadio "
                        +"where pa_idEquipo1 = E1.eq_id "
                        +"and pa_idEquipo2 = E2.eq_id "
                        +"and pa_idEstadio = es_id "
                        +"order by pa_fecha";

                List<Pronosticos> tablaPronosticos = db.Database.SqlQuery<Pronosticos>(querys).ToList<Pronosticos>();

                querys = "SELECT *"
                        + "FROM Marcador "
                        + "WHERE ma_idAlias=@idalias ";

                List<Marcador> pronosticosIngresados = db.Database.SqlQuery<Marcador>(querys, new SqlParameter("@idalias", id)).ToList<Marcador>();

                foreach (Marcador miPronostico in pronosticosIngresados){
                    foreach(Pronosticos itemPronostico in tablaPronosticos){

                        if(itemPronostico.id_partido == miPronostico.ma_idPartido){
                            itemPronostico.puntos = itemPronostico.CalcularPuntos(miPronostico);
                            itemPronostico.pronostico1 = miPronostico.ma_marcador1;
                            itemPronostico.pronostico2 = miPronostico.ma_marcador2;
                        }
                    }
                }

                querys = "SELECT *"
                         + "FROM AliasUsuario "
                         + "WHERE al_idUsuario=@iduser ";

                qvm.vm_alias = db.Database.SqlQuery<AliasUsuario>(querys, new SqlParameter("@iduser", DatosLogin.id_login)).ToList();

                qvm.vm_pronosticos = tablaPronosticos;

                return View(qvm);

            }else{
                return HttpNotFound();
            }


        }

        [Route("Quiniela/ActualizaPronostico")]
        [HttpPost]
        public string ActualizaPronostico(MarcadorPronostico miPronostico){
            string res = "Ocurrio un error inesperado";
            int idPartido = miPronostico.idPartido;

            /*
             * Aqui debe de ir una validación de sesión
            */

            try{
                querys = "SELECT *"
                        + "FROM Partido "
                        + "WHERE pa_id=@idpartido ";

                Partido partido = db.Database.SqlQuery<Partido>(querys, new SqlParameter("@idpartido", idPartido)).FirstOrDefault();

                if (partido.pa_estado == "V")
                {
                    querys = "SELECT *"
                            + "FROM Marcador "
                            + "WHERE ma_idAlias=@idalias "
                            + "AND ma_idPartido = @idpartido ";

                    Marcador marcador = db.Database.SqlQuery<Marcador>(querys, new SqlParameter("@idalias", miPronostico.idAlias), new SqlParameter("@idpartido", miPronostico.idPartido)).FirstOrDefault();

                    if (marcador != null)
                    {
                        querys = "update Marcador "
                                + "set ma_marcador1 = @marcador1, ma_marcador2 = @marcador2, ma_hora = getdate() "
                                + "WHERE ma_idAlias=@idalias "
                                + "AND ma_idPartido = @idpartido ";

                        db.Database.ExecuteSqlCommand(querys, new SqlParameter("@marcador1", miPronostico.marcador1), new SqlParameter("@marcador2", miPronostico.marcador2), new SqlParameter("@idalias", miPronostico.idAlias), new SqlParameter("@idpartido", miPronostico.idPartido));

                        res = "OK";
                    }
                    else
                    {
                        querys = "insert into Marcador(ma_idAlias, ma_idEquipo1, ma_idEquipo2, ma_marcador1, ma_marcador2, ma_idPartido, ma_estado, ma_fecha, ma_hora) "
                                + "values(@idalias, @idequipo1, @idequipo2, @marcador1, @marcador2, @idpartido, 'V', convert(varchar(10), getdate(), 101), getdate()) ";

                        db.Database.ExecuteSqlCommand(querys, new SqlParameter("@idalias", miPronostico.idAlias), new SqlParameter("@idequipo1", miPronostico.idEquipo1), new SqlParameter("@idequipo2", miPronostico.idEquipo2), new SqlParameter("@marcador1", miPronostico.marcador1), new SqlParameter("@marcador2", miPronostico.marcador2), new SqlParameter("@idpartido", miPronostico.idPartido));

                        res = "OK";
                    }
                }else{
                    res = "Ya no se pueden ingresar pronosticos para este partido :(";
                }
            }catch(Exception ex){
                res = ex.Message;
                res = "Ocurrio un error al actualizar el pronostico";
            }

            return res;
        }

        [Route("Quiniela/ActualizaMarcador")]
        [HttpPost]
        public bool ActualizaMarcador(MarcadorFinalLive marcadorfinal)
        {
            bool respuesta = false;
            int idPartido = marcadorfinal.idPartido;

            querys = "SELECT *"
                        + "FROM Partido "
                        + "WHERE pa_id=@idpartido ";

            Partido partido = db.Database.SqlQuery<Partido>(querys, new SqlParameter("@idpartido", idPartido)).FirstOrDefault();

            if (partido.pa_estado == "I")
            {
                querys = "update Partido "
                           + "set pa_marcador1 = @marcador1, pa_marcador2 = @marcador2 "
                           + "WHERE pa_id = @idpartido ";

                db.Database.ExecuteSqlCommand(querys, new SqlParameter("@marcador1", marcadorfinal.marcador1), new SqlParameter("@marcador2", marcadorfinal.marcador2), new SqlParameter("@idpartido", marcadorfinal.idPartido));
                //Partido upd = db.Database.SqlQuery<Partido>(querys, new SqlParameter("@marcador1", marcadorfinal.marcador1), new SqlParameter("@marcador2", marcadorfinal.marcador2),  new SqlParameter("@idpartido", marcadorfinal.idPartido)).FirstOrDefault();

                respuesta = true;

            }

            return respuesta;
        }
    }
}
