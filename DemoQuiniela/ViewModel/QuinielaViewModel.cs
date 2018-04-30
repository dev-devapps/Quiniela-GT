using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MvcQuiniela.Models;

namespace MvcQuiniela.ViewModel
{
    public class QuinielaViewModel
    {
        public List<AliasUsuario> vm_alias;

        public List<TablaPosiciones> vm_tablaPosiciones;

        public List<Pronosticos> vm_pronosticos;

        public List<MarcadorFinal> vm_marcadorFinales;

        public List<Usuario> vm_usuarios;

        public List<int> permisos;
    }
}