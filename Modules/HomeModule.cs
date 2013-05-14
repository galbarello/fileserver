using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using FileServer.Model;
using Nancy;

namespace FileServer
{
    public class HomeModule : NancyModule
    {       
        dynamic _tablaGrupos=new TablaGrupo();
        dynamic _cms = new TablaGruposCMS();

        public HomeModule()
        {
            Get[@"/"] = x =>
            {
                var indexViewModel = new IndexViewModel { CurrentDateTime = DateTime.Now, WorkLog = new List<string> { "pepe", "tito" } };

                return View["home", indexViewModel];
            };

            Post[@"/grupos/"] = x =>
            {
                string nombre = Request.Form.Nombre.ToString();
                
                else
                {
                    _tablaGrupos.Insert(
                        new
                        {
                            Id = ((string)Request.Form.Nombre).GenerateSequence(),
                            Nombre = (string)Request.Form.Nombre,
                            Estado = 1,
                            Legajo_Referente = 0,
                            Avaya_Referente = 0,
                            Fecha_Alta = DateTime.Now
                        });
                    return HttpStatusCode.Accepted;
                }
            };

            Get[@"/grupos/{id}"] = x =>
            {
                var grupo = _tablaGrupos.Get(Id: int.Parse(x.id));
                if (grupo==null)return HttpStatusCode.NotFound;
                
                return Response.AsJson(new GrupoResponse((double)grupo.ID, grupo.NOMBRE, true));
                 
            };

            Get[@"/agroups/{fecha}/{avaya}/{grupo}"] = x =>
            {
                var fecha = DateTime.ParseExact(x.fecha.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);
                var asesor_dia=_cms.All(DIA:fecha,AVAYA_ASESOR:int.Parse(x.avaya),GRUPO_CMS:int.Parse(x.grupo));
                if (asesor_dia==null) return HttpStatusCode.NotFound;

                return Response.AsJson(new AgroupsResponse((double)asesor_dia.AVAYA_ASESOR, asesor_dia.GRUPO_CMS,asesor_dia.DIA));
            };

            Post[@"/agroups/"] = x =>
            {
                dynamic record = new ExpandoObject();

                record.ID = DateTime.Now.Ticks;
                record.DIA = DateTime.ParseExact(Request.Form.Id.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);
                record.ACD_NO = 1;
                record.ESTADO = 1;
                record.ITEM_TYPE = Request.Form.Desc.ToString();
                record.FECHA_IMPORTADO = DateTime.Now;
                record.AVAYA_ASESOR = int.Parse(Request.Form.IdAvaya);
                record.GRUPO_CMS = ((string)Request.Form.Nombre).GenerateSequence();

                _cms.Insert(record);
                return HttpStatusCode.Accepted;
            };
                        
            Put[@"/grupos/(?<id>[\d])"] = x=>{return HttpStatusCode.NotFound;};
            Delete[@"/grupos/(?<id>[\d])"] = x=>{return HttpStatusCode.NotFound;};

        }        
    }
}
