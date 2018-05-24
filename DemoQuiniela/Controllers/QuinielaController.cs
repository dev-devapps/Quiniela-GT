using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Web.Mvc;
using MvcQuiniela.Models;
using System.Data.SqlClient;
using MvcQuiniela.ViewModel;
using System.Xml;
using System.Configuration;

namespace DemoQuiniela.Controllers
{
    public class QuinielaController : Controller
    {
        private QuinielaDBContext db = new QuinielaDBContext();
        private GoogleLogin googleParameters = new GoogleLogin();
        private GoogleUserOutputData userLogin = new GoogleUserOutputData();
        private User DatosLogin = new User();
        private string querys;
        private string urlLogout = "~/Quiniela/Salir/";

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

        [SessionCheck(Transaccion = 4)]
        public ActionResult Ayuda()
        {
            DatosLogin = (User)TempData["DatosLogin"];
            ViewBag.DatosLogin = DatosLogin;
            DatosLogin.id_menu = 4;

            return View();
        }

        public Object NullHandler(Object instance)
        {
            if (instance != null)
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
            int id_rol = 0;

            List<AliasUsuario> aliasDB = new List<AliasUsuario>();
            List<Usuario> userDB = new List<Usuario>();
            List<UsuarioRol> userRol = new List<UsuarioRol>();
            List<TransaccionRol> tranRol = new List<TransaccionRol>();
            List<int> permisosMenu = new List<int>();
            List<String> premiosFe = new List<String>();

            if (url != "")
            {
                userLogin = googleParameters.ObtenerCorreo(true, url);

                if (userLogin.email != null)
                {
                    querys = "SELECT *"
                         + "FROM Usuario "
                         + "WHERE us_correoElectronico=@email "
                         + "AND us_estado = 'V'";

                    userDB = db.Usuarios.SqlQuery(querys, new SqlParameter("@email", userLogin.email)).ToList();

                    if (userDB.Count == 1)
                    {
                        id_user = userDB.ElementAt(0).us_id;

                        querys = "SELECT *"
                         + "FROM UsuarioRol "
                            + "WHERE ur_idUsuario=@id " +
                            "AND ur_estado = 'V'";

                        userRol = db.UsuarioRol.SqlQuery(querys, new SqlParameter("@id", id_user)).ToList();

                        if (userRol.Count == 0) {
                            return Redirect(urlLogout);
                        } else {
                            id_rol = userRol.ElementAt(0).ur_idRol;

                            DatosLogin.email = userLogin.email;
                            DatosLogin.picture = userLogin.picture;
                            DatosLogin.id_login = id_user;
                            DatosLogin.login = true;
                            DatosLogin.id_menu = 1;
                            DatosLogin.id_rol = id_rol;
                            DatosLogin.nombre = userDB.ElementAt(0).us_primerNombre;
                            

                            if (id_user > 0)
                            {
                                Session["UserInfo"] = DatosLogin;

                                querys = "select * "
                                + "from TransaccionRol "
                                + "where tr_id_rol = @idrol";

                                tranRol = db.TransaccionRol.SqlQuery(querys, new SqlParameter("idrol", id_rol)).ToList();

                                foreach (TransaccionRol trn in tranRol) {
                                    permisosMenu.Add(trn.tr_id_transaccion);
                                }

                                DatosLogin.permisos = permisosMenu;

                                querys = "select convert(varchar(20), convert(decimal(6,2), (count(1) * 50) * 0.5))  primerLugar, convert(varchar(20), convert(decimal(6,2),(count(1) * 50) * 0.3))  segundoLugar, convert(varchar(20), convert(decimal(6,2), (count(1) * 50) * 0.2)) tercerLugar "
                                       + "from AliasUsuario, Usuario "
                                       + "where al_estado = 'V' "
                                       + "and al_idUsuario = us_id "
                                       + "and us_estado = 'V' ";

                                  List <Premios> premios = db.Database.SqlQuery<Premios>(querys).ToList<Premios>();

                                foreach (Premios prem in premios)
                                {
                                    premiosFe.Add(prem.primerLugar);
                                    premiosFe.Add(prem.segundoLugar);
                                    premiosFe.Add(prem.tercerLugar);
                                }

                                DatosLogin.premios = premiosFe;

                                querys = "SELECT *"
                                + "FROM AliasUsuario "
                                + "WHERE al_idUsuario=@iduser "
                                + "AND  al_codigoDeposito is not null "
                                + "AND al_estado = 'V'";

                                aliasDB = db.AliasUsuario.SqlQuery(querys, new SqlParameter("@iduser", id_user)).ToList();

                                if (aliasDB.Count == 1)
                                {
                                    AliasUsuario alias = aliasDB.First();

                                    return Redirect("/Quiniela/Posiciones/" + alias.al_id.ToString());
                                }else{
                                    if(aliasDB.Count == 0){
                                        return Redirect(urlLogout);
                                    }
                                }
                            }
                        }
                    }else{
                        return Redirect(urlLogout);
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

            List<Rol> rolesDB = new List<Rol>();
            QuinielaViewModel qvm = new QuinielaViewModel();

            querys = "SELECT *"
                    + "FROM Rol ";

            rolesDB = db.Database.SqlQuery<Rol>(querys).ToList();
            qvm.vm_roles = rolesDB;
            return View(qvm);
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
            querys = "SELECT *"
                    + "FROM UsuarioRol "
                    + "WHERE ur_idUsuario=@id "
                    + "AND ur_estado = 'V'";
            List<UsuarioRol> tablaUsuarioRol = db.Database.SqlQuery<UsuarioRol>(querys, new SqlParameter("@id", id)).ToList<UsuarioRol>();
            qvm.vm_usuarioRol = tablaUsuarioRol;


            List<Rol> rolesDB = new List<Rol>();
            querys = "SELECT *"
                    + "FROM Rol ";

            rolesDB = db.Database.SqlQuery<Rol>(querys).ToList();
            qvm.vm_usuarios = tablaUsuarios;
            qvm.vm_roles = rolesDB;
            
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

            if(DatosLogin == null){
                DatosLogin = (User)Session["UserInfo"];
            }

            if (DatosLogin != null && DatosLogin.login)
            {
                QuinielaViewModel vm = new QuinielaViewModel();

                querys = "SELECT *"
                        + "FROM AliasUsuario "
                        + "WHERE al_id=@idalias "
                        + "AND al_estado = 'V' ";

                AliasUsuario aliasSeleccionado = db.Database.SqlQuery<AliasUsuario>(querys, new SqlParameter("@idalias", id)).FirstOrDefault();

                if (aliasSeleccionado != null)
                {
                    DatosLogin.id_alias = id;
                    DatosLogin.nickname = aliasSeleccionado.al_nickname;

                    querys = "SELECT *"
                             + "FROM AliasUsuario "
                             + "WHERE al_idUsuario=@iduser "
                             + "AND  al_codigoDeposito is not null "
                             + "AND al_estado = 'V'";

                    vm.vm_alias = db.Database.SqlQuery<AliasUsuario>(querys, new SqlParameter("@iduser", aliasSeleccionado.al_idUsuario)).ToList();

                    querys = "SELECT id_alias=al_id, alias=al_nickname, puntos = al_puntos "
                                 + "FROM AliasUsuario "
                                 + "WHERE al_codigoDeposito is not null "
                                 + "AND al_estado = 'V' ";

                    List<TablaPosiciones> tablaPosiciones = db.Database.SqlQuery<TablaPosiciones>(querys).ToList<TablaPosiciones>();

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
                        + "and pa_estado in ('C', 'I', 'T') "
                        + "and al_estado = 'V' "
                        + "order by pa_fecha; ";
                
                List<Pronosticos> tablaPronosticos = db.Database.SqlQuery<Pronosticos>(querys, new SqlParameter("@id_partido", id)).ToList<Pronosticos>();
                if (tablaPronosticos.Count > 0)
                {
                    foreach (Pronosticos itemPronostico in tablaPronosticos)
                    {
                    itemPronostico.puntos = itemPronostico.CalcularPuntosDetallePartido();
                    }
                

                    qvm.vm_pronosticos = tablaPronosticos;

                    return View(qvm);
                }else
                {
                    return Redirect("/Quiniela/Error");
                }
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
                        + "and pa_estado in ('I', 'T') "
                        + "and al_estado = 'V' "
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
                     + "AND al_id=@idalias "
                     + "AND al_estado='V'";

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
                         + "WHERE al_idUsuario=@iduser "
                         + "AND al_estado='V'";

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


        public ActionResult Salir()
        {
            Session["UserInfo"] = null;
            return View();
        }


        [Route("Quiniela/GuardarUsuario")]
        [HttpPost]
        [SessionCheck(Transaccion = 6)]
        public JsonResult  GuardarUsuario(Usuario usuario, int rol, int idUsuario)
        {
            int Resultado = 0;
           // querys = "insert into Usuario values(@primerNombre, @segundoNombre, @primerApellido, @segundoApellido, @correoElectronico, @identificacion, @estado)";
            //db.Database.ExecuteSqlCommand(querys, new SqlParameter("@primerNombre", NullHandler(usuario.us_primerNombre)), new SqlParameter("@segundoNombre", NullHandler(usuario.us_segundoNombre)), new SqlParameter("@primerApellido", NullHandler(usuario.us_primerApellido)), new SqlParameter("@segundoApellido", NullHandler(usuario.us_segundoApellido)), new SqlParameter("@correoElectronico", NullHandler(usuario.us_correoElectronico)), new SqlParameter("@identificacion", NullHandler(usuario.us_cui)), new SqlParameter("@estado", 'V'));

            querys = "EXEC	quiniela..sp_usuario_mant "
                    + "@operacion = 'I',"
                    + "@primerNombre = @txtPrimerNombre,"
                    + "@segundoNombre = @txtSegundoNombre,"
                    + "@primerApellido = @txtPrimerApellido,"
                    + "@segundoApellido = @txtSegundoApellido,"
                    + "@correoElectronico = @txtCorreo,"
                    + "@cui = @txtCui,"
                    + "@estado = @txtEstado,"
                    + "@rol = @txtRol,"
                    + "@id  = '',"
                    + "@idAlias = '',"
                    + "@alias = '',"
                    + "@deposito = '',"
                    + "@usuario = @usuario";

            Resultado = db.Database.ExecuteSqlCommand(querys, new SqlParameter("@txtPrimerNombre", NullHandler(usuario.us_primerNombre)), new SqlParameter("@txtSegundoNombre", NullHandler(usuario.us_segundoNombre)), new SqlParameter("@txtPrimerApellido", NullHandler(usuario.us_primerApellido)), new SqlParameter("@txtSegundoApellido", NullHandler(usuario.us_segundoApellido)), new SqlParameter("@txtCorreo", NullHandler(usuario.us_correoElectronico)), new SqlParameter("@txtCui", NullHandler(usuario.us_cui)), new SqlParameter("@txtEstado", 'V'),new SqlParameter("@txtRol", rol), new SqlParameter("@usuario", idUsuario));
            if (Resultado == 0) {
                return Json(new { success = true, responseText = "true" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Error = false, responseText = "false" }, JsonRequestBehavior.AllowGet);
            }
            
        }

        [Route("Quiniela/ModificarUsuario")]
        [HttpPost]
        [SessionCheck(Transaccion = 6)]
        public JsonResult ModificarUsuario(Usuario usuario, int rol, int idUsuario)
        {
            querys = "EXEC	quiniela..sp_usuario_mant "
                    + "@operacion = 'U',"
                    + "@primerNombre = @primerNombre,"
                    + "@segundoNombre = @segundoNombre,"
                    + "@primerApellido = @primerApellido,"
                    + "@segundoApellido = @segundoApellido,"
                    + "@correoElectronico = @correoElectronico,"
                    + "@cui = @identificacion,"
                    + "@estado = @estado,"
                    + "@rol = @rol,"
                    + "@id  = @id,"
                   + "@idAlias = '',"
                   + "@alias = '',"
                   + "@deposito = '',"
                   + "@usuario = @usuario";


            db.Database.ExecuteSqlCommand(querys, new SqlParameter("@primerNombre", NullHandler(usuario.us_primerNombre)), new SqlParameter("@segundoNombre", NullHandler(usuario.us_segundoNombre)), new SqlParameter("@primerApellido", NullHandler(usuario.us_primerApellido)), new SqlParameter("@segundoApellido", NullHandler(usuario.us_segundoApellido)), new SqlParameter("@correoElectronico", NullHandler(usuario.us_correoElectronico)), new SqlParameter("@identificacion", NullHandler(usuario.us_cui)), new SqlParameter("@estado", NullHandler(usuario.us_estado)), new SqlParameter("@rol", NullHandler(rol)), new SqlParameter("@id", NullHandler(usuario.us_id)), new SqlParameter("@usuario", idUsuario));

            return Json(new { success = true, responseText = "true" }, JsonRequestBehavior.AllowGet);
        }

        [Route("Quiniela/GuardarAlias")]
        [HttpPost]
        [SessionCheck(Transaccion = 6)]
        public JsonResult GuardarAlias(AliasUsuario alias, int idUsuario)
        {

            querys = "EXEC	quiniela..sp_usuario_mant "
                   + "@operacion = 'AI',"
                   + "@primerNombre = '',"
                   + "@segundoNombre = '',"
                   + "@primerApellido = '',"
                   + "@segundoApellido = '',"
                   + "@correoElectronico = '',"
                   + "@cui = '',"
                   + "@estado = 'V',"
                   + "@id=@id_usuario,"
                   + "@idAlias = '',"
                   + "@alias = @alias,"
                   + "@deposito = @boleta,"
                   + "@usuario = @usuario";

            db.Database.ExecuteSqlCommand(querys, new SqlParameter("@id_usuario", NullHandler(alias.al_idUsuario)), new SqlParameter("@alias", NullHandler(alias.al_nickname)), new SqlParameter("@boleta", NullHandler(alias.al_codigoDeposito)), new SqlParameter("@usuario", idUsuario));

            return Json(new { success = true, responseText = "true" }, JsonRequestBehavior.AllowGet);
        }
        [Route("Quiniela/ModificarAlias")]
        [HttpPost]
        [SessionCheck(Transaccion = 6)]
        public JsonResult ModificarAlias(AliasUsuario alias, int idUsuario)
        {
            querys = "EXEC	quiniela..sp_usuario_mant "
                   + "@operacion = 'AU',"
                   + "@primerNombre = '',"
                   + "@segundoNombre = '',"
                   + "@primerApellido = '',"
                   + "@segundoApellido = '',"
                   + "@correoElectronico = '',"
                   + "@cui = '',"
                   + "@estado = @estado,"
                   + "@id=@idUsuario,"
                   + "@idAlias = @id,"
                   + "@alias = @nickname,"
                   + "@deposito = @numeroDeposito,"
                   + "@usuario = @usuario";

            db.Database.ExecuteSqlCommand(querys, new SqlParameter("@nickname", NullHandler(alias.al_nickname)), new SqlParameter("@numeroDeposito", NullHandler(alias.al_codigoDeposito)), new SqlParameter("@estado", NullHandler(alias.al_estado)), new SqlParameter("@id", NullHandler(alias.al_id)), new SqlParameter("@idUsuario", NullHandler(alias.al_idUsuario)), new SqlParameter("@usuario", idUsuario));

            return Json(new { success = true, responseText = "true" }, JsonRequestBehavior.AllowGet);
        }

        [Route("Quiniela/CorreoBienvenida")]
        [HttpPost]
        [SessionCheck(Transaccion = 6)]
        public JsonResult CorreoBienvenida(int n)
        {
            string resEnvioMail = "", mensaje = "", querys = "";
            string c = ConfigurationManager.AppSettings["C"].ToString(), p = ConfigurationManager.AppSettings["P"].ToString();

            EnvioCorreo mail = new EnvioCorreo();

            querys = "select * from Usuario where us_id = @idUsuario";
            Usuario usuario = db.Database.SqlQuery<Usuario>(querys, new SqlParameter("@idUsuario", n)).FirstOrDefault();

            if(usuario == null){
                return Json(new { Error = false, responseText = "Ocurrio un error al enviar el correo..." }, JsonRequestBehavior.AllowGet);
            }else{
                mail.HOST = "smtp.office365.com";
                mail.PORT = 587;

                mail.SMTP_USERNAME = c;
                mail.SMTP_PASSWORD = p;
                mail.ENABLESSL = true;

                mensaje = "<html>" +
                                    "<head>" +
                                        "<title>Quiniela</title>" +
                                    "</head>" +
                                    "<body>" +
                                    "<p style=\"font-family:Verdana;font-size:11px;\">" +
                                    "¡Bienvenid@ <b>" + usuario.us_primerNombre + "</b> a la Quiniela Rusia 2018!, es un gusto para nosotros contar con tu participaci&oacute;n en esta quiniela.<br /><br />" +
                                    "Para que puedas ingresar tus pron&oacute;sticos en la p&aacute;gina debes de ingresar al siguiente <a href=\"https://www.devappsgt.com/Quiniela\">link</a> o haz clic sobre la imagen, y para ingresar debes de hacerlo con el correo que te registraste.<br /><br /> " +
                                    "¡Te deseamos la mejor de las suertes!<br /><br />" +
                                    "<a href=\"https://www.devappsgt.com/Quiniela\"><img width=\"100%\" src=\"http://www.devappsgt.com/img/welcomemail.png\" border=\"0\" alt=\"Bienvenida\" /></a>" +
                                    "</p>" +
                                    "</body>" +
                           "</html>";


                resEnvioMail = mail.SendMail("Quiniela", c, usuario.us_correoElectronico, "", "", "Bienvenida a la Quiniela 2018", true, mensaje, "");

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(resEnvioMail);

                XmlNode errorNode = xmlDoc.DocumentElement.SelectSingleNode("/envio_correo/resultado");

                string errorCode = errorNode.Attributes["codigo"].Value;
                string errorMessage = errorNode.InnerText;

                if (errorCode == "1")
                {
                    return Json(new { success = true, responseText = "true" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, responseText = errorMessage }, JsonRequestBehavior.AllowGet);
                }
            }


        }

        [Route("Quiniela/ActualizaPronostico")]
        [HttpPost]
        [SessionCheck(Transaccion = 2)]
        public string ActualizaPronostico(MarcadorPronostico miPronostico){
            string res = "Ocurrio un error inesperado";
            int idPartido = miPronostico.idPartido;

            //ViewBag.DatosLogin = TempData["DatosLogin"];
            DatosLogin = (User)Session["UserInfo"];

            /*
             * Aqui debe de ir una validación de sesión
            */

            try{

                querys = "SELECT *"
                     + "FROM AliasUsuario "
                     + "WHERE al_id=@idalias "
                     + "AND al_estado='V'";

                //aliasDB = db.AliasUsuario.SqlQuery(querys, new SqlParameter("@iduser", DatosLogin.id_login), new SqlParameter("@idalias", id)).ToList();

                AliasUsuario aliasPronostico = db.Database.SqlQuery<AliasUsuario>(querys, new SqlParameter("@idalias", miPronostico.idAlias)).FirstOrDefault();

                if (aliasPronostico.al_idUsuario != DatosLogin.id_login)
                {
                    return "Whoa, whoa, whoa... No hagas trampa porque seras descalificado :(";
                }

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

            

            if (partido.pa_estado == "I" ||  partido.pa_estado == "T")
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
