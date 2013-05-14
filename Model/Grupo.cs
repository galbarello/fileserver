using Massive;
using System;

namespace FileServer.Model
{
    public class TablaGrupo:DynamicModel
    {
        public TablaGrupo()
            : base("oracle", "GRUPOS", "ID")
        { }
    }

    public class TablaGruposCMS : DynamicModel
    {
        public TablaGruposCMS()
            : base("oracle", "CMS_AGROUPS","PEPE")
        { }
    }

    public class TablaEmpleado : DynamicModel
    {
        public TablaEmpleado()
            : base("oracle", "EMPLEADO", "ID")
        { }
    }

    public class TablaSap : DynamicModel
    {
        public TablaSap()
            : base("oracle", "SAP","LEGAJO")
        { }
    }

    public class TablaAvaya : DynamicModel
    {
        public TablaAvaya()
            : base("oracle", "AVAYA", "ID")
        { }
    }

    public class AgroupsResponse
    {
        public readonly double IdAvaya;
        public readonly double Grupo;
        public readonly DateTime Dia;

        public AgroupsResponse(double avaya,double grupo,DateTime fecha)
        {
            IdAvaya = avaya;
            Grupo = grupo;
            Dia = fecha;
        }

    }

    public class Automaticos
    {
        public readonly int Super;
        public readonly int Legajo;
        public readonly DateTime Fecha;

        public Automaticos(int super, int legajo, DateTime fecha)
        {
            Super = super;
            Legajo = legajo;
            Fecha = fecha;
        }
    }

    public class GrupoResponse
    {
        public readonly double id;
        public readonly string nombre;
        public bool success;

        public GrupoResponse(double id,string nombre,bool success)
        {
            this.id=id;
            this.nombre=nombre;
            this.success=success;
        }
    }
}
