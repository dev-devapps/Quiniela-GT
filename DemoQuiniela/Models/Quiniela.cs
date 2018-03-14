using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcQuiniela.Models
{
    [Table("Equipo")]
    public class Equipo
    {
       [Key] 
        public int eq_id { get; set; }
        public string eq_descripcion { get; set; }
        public string eq_estado { get; set; }
    }

    public class QuinielaDBContext : DbContext
    {
        public DbSet<Equipo> Equipos { get; set; }
    }
}
