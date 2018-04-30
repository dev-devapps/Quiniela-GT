using System;
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
        private string urlLogout = "~/Quiniela";

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

        public Object NullHandler(Object instance)
        {
            if(instance != null)
            {
                return instance;
            }
            else
            {
                return DBNull.Value;
            }
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
            List<UsuarioRol> userRol = new List<UsuarioRol>();

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
                        DatosLogin.id_menu = 1;

                        querys = "SELECT *"
                         + "FROM UsuarioRol "
                         + "WHERE ur_idUsuario=@id";

                        userRol = db.UsuarioRol.SqlQuery(querys, new SqlParameter("@id", id_user)).ToList();

                        if (id_user > 0)
                        {

                            DatosLogin.id_rol = userRol.ElementAt(0).ur_idRol;

                            Session["UserInfo"] = DatosLogin;

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
                    return Redirect(urlLogout);
                }

            }
            else
            {
                return Redirect(urlLogout);
            }

        }
        [SessionCheck(Transaccion = 6)]
        public ActionResult ListaUsuario()
        {
            ViewBag.DatosLogin = TempData["DatosLogin"];
            DatosLogin = (User)TempData["DatosLogin"];
            DatosLogin.id_menu = 6;

            QuinielaViewModel qvm = new QuinielaViewModel();

            querys = "SELECT * "
                               + "FROM Usuario ";
            List<Usuario> tablaUsuarios = db.Database.SqlQuery<Usuario>(querys).ToList<Usuario>();
            qvm.vm_usuarios = tablaUsuarios;
            return View(qvm);
        }
        [SessionCheck(Transaccion = 6)]
        public ActionResult IngresoUsuario()
        {
            ViewBag.DatosLogin = TempData["DatosLogin"];
            DatosLogin = (User)TempData["DatosLogin"];
            DatosLogin.id_menu = 6;

            QuinielaViewModel qvm = new QuinielaViewModel();
            return View();
        }
        [SessionCheck(Transaccion = 6)]
        public ActionResult EditarUsuario(int id)
        {
            ViewBag.DatosLogin = TempData["DatosLogin"];
            DatosLogin = (User)TempData["DatosLogin"];
            DatosLogin.id_menu = 6;

            QuinielaViewModel qvm = new QuinielaViewModel();
            querys = "SELECT * "
                     + "FROM Usuario "
                     + "WHERE us_id=@id";
            List<Usuario> tablaUsuarios = db.Database.SqlQuery<Usuario>(querys, new SqlParameter("@id", id)).ToList<Usuario>();
            qvm.vm_usuarios = tablaUsuarios;
            return View(qvm);
        }

        [SessionCheck(Transaccion = 6)]
        public ActionResult ListaAlias(int id)
        {
            
            string url = "";
            
            ViewBag.DatosLogin = TempData["DatosLogin"];
            DatosLogin = (User)TempData["DatosLogin"];
            DatosLogin.id_menu = 6;

            List<AliasUsuario> aliasDB = new List<AliasUsuario>();
            List<Usuario> usuarioDB = new List<Usuario>();
            QuinielaViewModel qvm = new QuinielaViewModel();

            querys = "SELECT *"
                    + "FROM Usuario "
                    + "WHERE us_id=@iduser ";

            usuarioDB = db.Database.SqlQuery<Usuario>(querys, new SqlParameter("@iduser", id)).ToList();

            querys = "SELECT *"
                    + "FROM AliasUsuario "
                    + "WHERE al_idUsuario=@iduser ";

            aliasDB = db.AliasUsuario.SqlQuery(querys, new SqlParameter("@iduser", id)).ToList();
            if (aliasDB.Count == 0)
            {
                url = "~/Quiniela/IngresoAlias/" + id.ToString();
                return Redirect(url);
            }
            qvm.vm_alias = aliasDB;
            qvm.vm_usuarios = usuarioDB;
            return View(qvm);

        }
        [SessionCheck(Transaccion = 6)]
        public ActionResult IngresoAlias(int id)
        {
            ViewBag.DatosLogin = TempData["DatosLogin"];
            DatosLogin = (User)TempData["DatosLogin"];
            DatosLogin.id_menu = 6;

            QuinielaViewModel qvm = new QuinielaViewModel();
            querys = "SELECT * "
                               + "FROM Usuario "
                               + "WHERE us_id=@idUser";
            List<Usuario> tablaUsuarios = db.Database.SqlQuery<Usuario>(querys, new SqlParameter("@idUser", id)).ToList<Usuario>();
            qvm.vm_usuarios = tablaUsuarios;
            return View(qvm);
            
        }
        [SessionCheck(Transaccion = 6)]
        public ActionResult EditarAlias(int id)
        {
            ViewBag.DatosLogin = TempData["DatosLogin"];
            DatosLogin = (User)TempData["DatosLogin"];
            DatosLogin.id_menu = 6;

            List<Usuario> usuarioDB = new List<Usuario>();

            QuinielaViewModel qvm = new QuinielaViewModel();

            querys = "SELECT * "
                     + "FROM AliasUsuario "
                     + "WHERE al_id=@id";

            List<AliasUsuario> aliasSeleccionado = db.Database.SqlQuery<AliasUsuario>(querys, new SqlParameter("@id", id)).ToList<AliasUsuario>();

            querys = "SELECT *"
                   + "FROM Usuario "
                   + "WHERE us_id=@iduser ";

            usuarioDB = db.Database.SqlQuery<Usuario>(querys, new SqlParameter("@iduser", aliasSeleccionado.ElementAt(0).al_idUsuario)).ToList();

            qvm.vm_usuarios = usuarioDB;
            qvm.vm_alias = aliasSeleccionado;

            return View(qvm);
        }
        [SessionCheck(Transaccion = 1)]
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
                    DatosLogin.id_menu = 1;

                    ViewBag.DatosLogin = DatosLogin;

                    return View(vm);
                }
                else
                {
                    return Redirect(urlLogout);
                }
            }
            else
            {
                return Redirect(urlLogout);
            }
        }

        [SessionCheck(Transaccion = 1)]
        public ActionResult DetallePorPartido(int id)
        {
            ViewBag.DatosLogin = TempData["DatosLogin"];
            DatosLogin = (User)TempData["DatosLogin"];
            List<AliasUsuario> aliasDB = new List<AliasUsuario>();
            Marcador miPronostico = new Marcador();

            QuinielaViewModel qvm = new QuinielaViewModel();

            if (DatosLogin != null && DatosLogin.login)
            {


                querys = "select pa_id, al_id, id_equipo1 = pa_idEquipo1, equipo1 = E1.eq_descripcion, marcador1 = pa_marcador1, pronostico1 = ma_marcador1, id_equipo2 = pa_idEquipo2, equipo2 = E2.eq_descripcion, marcador2 = pa_marcador2, pronostico2 = ma_marcador2, puntos = isnull(al_puntos, 0), id_estadio = es_id, estadio = es_nombre, fecha = convert(varchar(10), pa_fecha, 103), hora = convert(varchar(5), pa_hora, 108), estado = pa_estado, alias = al_nickname "
                        + "from Partido, Equipo as E1, Equipo as E2, Estadio, Marcador, AliasUsuario "
                        + "where pa_idEquipo1 = E1.eq_id "
                        + "and pa_idEquipo2 = E2.eq_id "
                        + "and pa_idEstadio = es_id "
                        + "and ma_idPartido = pa_id "
                        + "and ma_idEquipo1 = E1.eq_id "
                        + "and ma_idEquipo2 = E2.eq_id "
                        + "and ma_idAlias = al_id "
                        + "and pa_id = @id_partido "
                        + "order by pa_fecha; ";
                
                List<Pronosticos> tablaPronosticos = db.Database.SqlQuery<Pronosticos>(querys, new SqlParameter("@id_partido", id)).ToList<Pronosticos>();
                foreach (Pronosticos itemPronostico in tablaPronosticos)
                {
                    itemPronostico.puntos = itemPronostico.CalcularPuntosDetallePartido();
                }
                

                qvm.vm_pronosticos = tablaPronosticos;

                return View(qvm);

            }
            else
            {
                return Redirect(urlLogout);
            }

        }

        [SessionCheck(Transaccion = 1)]
        public ActionResult DetallePorAlias(int id)
        {
            ViewBag.DatosLogin = TempData["DatosLogin"];
            DatosLogin = (User)TempData["DatosLogin"];
            List<AliasUsuario> aliasDB = new List<AliasUsuario>();
            Marcador miPronostico = new Marcador();

            QuinielaViewModel qvm = new QuinielaViewModel();

            if (DatosLogin != null && DatosLogin.login)
            {


                querys = "select pa_id, al_id, id_equipo1 = pa_idEquipo1, equipo1 = E1.eq_descripcion, marcador1 = pa_marcador1, pronostico1 = ma_marcador1, id_equipo2 = pa_idEquipo2, equipo2 = E2.eq_descripcion, marcador2 = pa_marcador2, pronostico2 = ma_marcador2, puntos = isnull(al_puntos, 0), id_estadio = es_id, estadio = es_nombre, fecha = convert(varchar(10), pa_fecha, 103), hora = convert(varchar(5), pa_hora, 108), estado = pa_estado, alias = al_nickname "
                        + "from Partido, Equipo as E1, Equipo as E2, Estadio, Marcador, AliasUsuario "
                        + "where pa_idEquipo1 = E1.eq_id "
                        + "and pa_idEquipo2 = E2.eq_id "
                        + "and pa_idEstadio = es_id "
                        + "and ma_idPartido = pa_id "
                        + "and ma_idEquipo1 = E1.eq_id "
                        + "and ma_idEquipo2 = E2.eq_id "
                        + "and ma_idAlias = al_id "
                        + "and al_id = @id "
                        + "order by pa_fecha; ";

                List<Pronosticos> tablaPronosticos = db.Database.SqlQuery<Pronosticos>(querys, new SqlParameter("@id", id)).ToList<Pronosticos>();
                foreach (Pronosticos itemPronostico in tablaPronosticos)
                {
                    itemPronostico.puntos = itemPronostico.CalcularPuntosDetallePartido();
                }


                qvm.vm_pronosticos = tablaPronosticos;

                return View(qvm);

            }
            else
            {
                return Redirect(urlLogout);
            }

        }

        [SessionCheck(Transaccion = 5)]
        public ActionResult IngresoMarcadores()
        {
            ViewBag.DatosLogin = TempData["DatosLogin"];
            DatosLogin = (User)TempData["DatosLogin"];
            DatosLogin.id_menu = 5;

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
        [SessionCheck(Transaccion = 2)]
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

            //aliasDB = db.AliasUsuario.SqlQuery(querys, new SqlParameter("@iduser", DatosLogin.id_login), new SqlParameter("@idalias", id)).ToList();

            AliasUsuario aliasSeleccionado = db.Database.SqlQuery<AliasUsuario>(querys, new SqlParameter("@iduser", DatosLogin.id_login), new SqlParameter("@idalias", id)).FirstOrDefault();

            if(aliasSeleccionado == null){
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

                DatosLogin.id_alias = id;
                DatosLogin.nickname = aliasSeleccionado.al_nickname;
                DatosLogin.id_menu = 2;

                return View(qvm);

            }else{
                return Redirect(urlLogout);
            }


        }


        [Route("Quiniela/GuardarUsuario")]
        [HttpPost]
        [SessionCheck(Transaccion = 6)]
        public JsonResult  GuardarUsuario(Usuario usuario)
        {
            querys = "insert into Usuario values(@primerNombre, @segundoNombre, @primerApellido, @segundoApellido, @correoElectronico, @identificacion, @estado)";
            db.Database.ExecuteSqlCommand(querys, new SqlParameter("@primerNombre", NullHandler(usuario.us_primerNombre)), new SqlParameter("@segundoNombre", NullHandler(usuario.us_segundoNombre)), new SqlParameter("@primerApellido", NullHandler(usuario.us_primerApellido)), new SqlParameter("@segundoApellido", NullHandler(usuario.us_segundoApellido)), new SqlParameter("@correoElectronico", NullHandler(usuario.us_correoElectronico)), new SqlParameter("@identificacion", NullHandler(usuario.us_cui)), new SqlParameter("@estado", 'V'));
            return Json(new { success = true, responseText = "true" }, JsonRequestBehavior.AllowGet);
        }

        [Route("Quiniela/ModificarUsuario")]
        [HttpPost]
        [SessionCheck(Transaccion = 6)]
        public JsonResult ModificarUsuario(Usuario usuario)
        {
            
            querys = "update Usuario "
                           + "set us_primerNombre = @primerNombre, us_segundoNombre = @segundoNombre, us_primerApellido = @primerApellido, us_segundoApellido = @segundoApellido, us_correoElectronico = @correoElectronico, us_cui = @identificacion, us_estado = @estado "
                           + "WHERE us_id=@id ";
            db.Database.ExecuteSqlCommand(querys, new SqlParameter("@primerNombre", NullHandler(usuario.us_primerNombre)), new SqlParameter("@segundoNombre", NullHandler(usuario.us_segundoNombre)), new SqlParameter("@primerApellido", NullHandler(usuario.us_primerApellido)), new SqlParameter("@segundoApellido", NullHandler(usuario.us_segundoApellido)), new SqlParameter("@correoElectronico", NullHandler(usuario.us_correoElectronico)), new SqlParameter("@identificacion", NullHandler(usuario.us_cui)), new SqlParameter("@estado", NullHandler(usuario.us_estado)), new SqlParameter("@id", NullHandler(usuario.us_id)));

            return Json(new { success = true, responseText = "true" }, JsonRequestBehavior.AllowGet);
        }

        [Route("Quiniela/GuardarAlias")]
        [HttpPost]
        [SessionCheck(Transaccion = 6)]
        public JsonResult GuardarAlias(AliasUsuario alias)
        {
            querys = "insert into AliasUsuario values(@id_usuario, @alias, @boleta)";
            db.Database.ExecuteSqlCommand(querys, new SqlParameter("@id_usuario", NullHandler(alias.al_idUsuario)), new SqlParameter("@alias", NullHandler(alias.al_nickname)), new SqlParameter("@boleta", NullHandler(alias.al_codigoDeposito)));

            return Json(new { success = true, responseText = "true" }, JsonRequestBehavior.AllowGet);
        }
        [Route("Quiniela/ModificarAlias")]
        [HttpPost]
        [SessionCheck(Transaccion = 6)]
        public JsonResult ModificarAlias(AliasUsuario alias)
        {

            querys = "UPDATE AliasUsuario "
                           + "SET al_nickname = @nickname, al_codigoDeposito = @numeroDeposito, al_estado = @estado "
                           + "WHERE al_id=@id "
                           + "AND al_idUsuario=@idUsuario";
            db.Database.ExecuteSqlCommand(querys, new SqlParameter("@nickname", NullHandler(alias.al_nickname)), new SqlParameter("@numeroDeposito", NullHandler(alias.al_codigoDeposito)), new SqlParameter("@estado", NullHandler(alias.al_estado)), new SqlParameter("@id", NullHandler(alias.al_id)), new SqlParameter("@idUsuario", NullHandler(alias.al_idUsuario)));

            return Json(new { success = true, responseText = "true" }, JsonRequestBehavior.AllowGet);
        }

        [Route("Quiniela/ActualizaPronostico")]
        [HttpPost]
        [SessionCheck(Transaccion = 2)]
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
        [SessionCheck(Transaccion = 5)]
        public bool ActualizaMarcador(MarcadorFinalLive marcadorfinal)
        {
            bool respuesta = false;
            int idPartido = marcadorfinal.idPartido;
            int Resultado;

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
                querys = "EXEC sp_actualiza_puntos ";
                Resultado = db.Database.ExecuteSqlCommand(querys);

                respuesta = true;

            }

            return respuesta;
        }
    }
}
