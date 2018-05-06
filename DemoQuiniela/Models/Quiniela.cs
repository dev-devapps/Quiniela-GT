using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using Newtonsoft.Json;

namespace MvcQuiniela.Models
{
    public class QuinielaDBContext : DbContext
    {
        public DbSet<Equipo> Equipos { get; set; }

        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<AliasUsuario> AliasUsuario { get; set; }

        public DbSet<Partido> Partido { get; set; }

        public DbSet<Marcador> Marcador { get; set; }

        public DbSet<UsuarioRol> UsuarioRol { get; set; }

        public DbSet<TransaccionRol> TransaccionRol { get; set; }

        public DbSet<Rol> Rol { get; set; }

    }

    [Table("Equipo")]
    public class Equipo
    {
        [Key]
        public int eq_id { get; set; }
        public string eq_descripcion { get; set; }
        public string eq_estado { get; set; }
    }


    [Table("Rol")]
    public class Rol
    {
        [Key]
        public int ro_id { get; set; }
        public string ro_nombre { get; set; }
        public string ro_estado { get; set; }
    }


    [Table("UsuarioRol")]
    public class UsuarioRol
    {
        [Key]
        [Column(Order = 0)]
        public int ur_idUsuario { get; set; }
        [Key]
        [Column(Order = 1)]
        public int ur_idRol { get; set; }
        public string ur_estado { get; set; }
    }

    [Table("TransaccionRol")]
    public class TransaccionRol
    {
        [Key]
        [Column(Order = 0)]
        public int tr_id_transaccion { get; set; }
        [Key]
        [Column(Order = 1)]
        public int tr_id_rol { get; set; }
    }

    [Table("Usuario")]
    public class Usuario
    {
        [Key]
        public int us_id { get; set; }
        public string us_primerNombre { get; set; }

        public string us_segundoNombre { get; set; }

        public string us_primerApellido { get; set; }

        public string us_segundoApellido { get; set; }

        public string us_correoElectronico { get; set; }

        public string us_cui { get; set; }
        public string us_estado { get; set; }


    }

    [Table("AliasUsuario")]
    public class AliasUsuario
    {
        [Key]
        public int al_id { get; set; }

        public int al_idUsuario { get; set; }

        public string al_nickname { get; set; }

        public int al_codigoDeposito { get; set; }
        public int al_puntos { get; set; }

        public string al_estado { get; set; }
    }

    [Table("Partido")]
    public class Partido
    {
        [Key]
        public int pa_id { get; set; }

        public int pa_idEquipo1 { get; set; }

        public int pa_idEquipo2 { get; set; }

        public int pa_marcador1 { get; set; }

        public int pa_marcador2 { get; set; }

        public int pa_idFaseTorneo { get; set; }

        public int pa_idEstadio { get; set; }

        public DateTime pa_fecha { get; set; }

        public DateTime pa_hora { get; set; }

        public string pa_estado { get; set; }
    }

    [Table("Marcador")]
    public class Marcador
    {
        [Key]
        public int ma_idAlias { get; set; }

        public int ma_idEquipo1 { get; set; }

        public int ma_idEquipo2 { get; set; }

        public int ma_marcador1 { get; set; }

        public int ma_marcador2 { get; set; }

        public int ma_idPartido { get; set; }

        public string ma_estado { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ma_fecha { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ma_hora { get; set; }

    }

    public class MarcadorPronostico
    {
        public int idAlias { get; set; }

        public int idEquipo1 { get; set; }

        public int idEquipo2 { get; set; }

        public int marcador1 { get; set; }

        public int marcador2 { get; set; }

        public int idPartido { get; set; }
    }

    public class MarcadorFinalLive
    {
        public int idEquipo1 { get; set; }

        public int idEquipo2 { get; set; }

        public int marcador1 { get; set; }

        public int marcador2 { get; set; }

        public int idPartido { get; set; }
    }
    public class TablaPosiciones
    {

        public int id_alias { get; set; }

        public string alias { get; set; }

        public int puntos { get; set; }

        public int CalcularPuntos(Marcador pronostico, Partido partidoJugado)
        {
            int puntos = 0;

            if (partidoJugado.pa_idEquipo1 == pronostico.ma_idEquipo1 && partidoJugado.pa_idEquipo2 == pronostico.ma_idEquipo2)
            {

                if (partidoJugado.pa_marcador1 == pronostico.ma_marcador1 && partidoJugado.pa_marcador2 == pronostico.ma_marcador2)
                {
                    puntos = 5;
                }
                else
                {
                    if (partidoJugado.pa_marcador1 == pronostico.ma_marcador1)
                        puntos++;

                    if (partidoJugado.pa_marcador2 == pronostico.ma_marcador2)
                        puntos++;

                    if (partidoJugado.pa_marcador1 > partidoJugado.pa_marcador2 && pronostico.ma_marcador1 > pronostico.ma_marcador2)
                        puntos = puntos + 3;

                    if (partidoJugado.pa_marcador2 > partidoJugado.pa_marcador1 && pronostico.ma_marcador2 > pronostico.ma_marcador1)
                        puntos = puntos + 3;

                    if (partidoJugado.pa_marcador2 == partidoJugado.pa_marcador1 && pronostico.ma_marcador2 == pronostico.ma_marcador1)
                        puntos = 3;
                }

            }
            
            return puntos;
        }

    }

    public class Pronosticos
    {
        public int id_partido { get; set; }

        public int id_alias { get; set; }

        public int id_equipo1 { get; set; }

        public string equipo1 { get; set; }

        public int marcador1 { get; set; }

        public int pronostico1 { get; set; }

        public int id_equipo2 { get; set; }

        public string equipo2 { get; set; }

        public int marcador2 { get; set; }

        public int pronostico2 { get; set; }

        public int puntos { get; set; }

        public int id_estadio { get; set; }

        public string estadio { get; set; }

        public string fecha { get; set; }

        public string hora { get; set; }

        public string estado { get; set; }

        public string alias { get; set; }

        public int CalcularPuntos(Marcador pronostico)
        {
            int rPuntos = 0;

            if (pronostico.ma_marcador1 == this.marcador1 && pronostico.ma_marcador2 == this.marcador2)
            {
                rPuntos = 5;
            }
            else
            {
                if (this.marcador1 == pronostico.ma_marcador1)
                    rPuntos++;

                if (this.marcador2 == pronostico.ma_marcador2)
                    rPuntos++;

                if (this.marcador1 == this.marcador2 && pronostico.ma_marcador1 == pronostico.ma_marcador2)
                    rPuntos += 3;
                
                if (this.marcador1 > this.marcador2 && pronostico.ma_marcador1 > pronostico.ma_marcador2)
                    rPuntos += 3;

                if (this.marcador2 > this.marcador1 && pronostico.ma_marcador2 > pronostico.ma_marcador1)
                    rPuntos += 3;
            }


            return rPuntos;
        }
        public int CalcularPuntosDetallePartido()
        {
            int rPuntos = 0;

            if (this.pronostico1 == this.marcador1 && this.pronostico2 == this.marcador2)
            {
                rPuntos = 5;
            }
            else
            {
                if (this.marcador1 == this.pronostico1)
                    rPuntos++;

                if (this.marcador2 == this.pronostico2)
                    rPuntos++;

                if (this.marcador1 == this.marcador2 && this.pronostico1 == this.pronostico2)
                    rPuntos += 3;

                if (this.marcador1 > this.marcador2 && this.pronostico1 > this.pronostico2)
                    rPuntos += 3;

                if (this.marcador2 > this.marcador1 && this.pronostico2 > this.pronostico1)
                    rPuntos += 3;
            }


            return rPuntos;
        }

    }

    public class MarcadorFinal
    {
        public int id_partido { get; set; }

        public int id_equipo1 { get; set; }

        public string equipo1 { get; set; }

        public int marcador1 { get; set; }

        public int id_equipo2 { get; set; }

        public string equipo2 { get; set; }

        public int marcador2 { get; set; }
        
        public int id_estadio { get; set; }

        public string estadio { get; set; }

        public string fecha { get; set; }

        public string hora { get; set; }

        public string estado { get; set; }

    }

}
