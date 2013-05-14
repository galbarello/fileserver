using System;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FileHelpers;
using FileServer.Files;
using FileServer.Model;
using Massive;
using System.Collections.Generic;

namespace FileServer.Processors
{
    public class GruposProcessor
    {
        dynamic _db = DynamicModel.Open("oracle");
        dynamic _cms = new TablaGruposCMS();
        dynamic _sap = new TablaSap();
        dynamic _dotacion = new TablaEmpleado();

        FileHelperEngine<Grupo> _engine = new FileHelperEngine<Grupo>();        

        public void Process(string file)
        {
            var integrantes = _engine.ReadFile(file).ToList();

            var lista = (from p in integrantes
                         group p by p.Nombre into grp
                         select new { Nombre = grp.Key, Id = grp.Key.ToString().GenerateSequence() })
                         .ToList();

            var fecha = DateTime.ParseExact((from agente in integrantes select agente.Id).Max().ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);           
                        

            lista.AsParallel().ForAll(
                grupo => InsertGrupoIfNecessary(
                    grupo.Id,
                    grupo.Nombre));


            integrantes.AsParallel().ForAll(integrante =>
            {
                InsertAgroupsIfNecessary(
                    integrante.Id.ToString(),
                    int.Parse(integrante.IdAvaya),
                    integrante.Desc.ToString(),
                    integrante.Nombre.ToString());
            });           
            

            var gruposAutomaticos = _db.Query(
            @" SELECT DISTINCT AGROUPS.GRUPO_CMS, GRUPOS.LEGAJO_REFERENTE, AVAYA.LEGAJO
               FROM  (SELECT MAX(DIA) AS DIA, GRUPO_CMS, AVAYA_ASESOR
               FROM   CMS_AGROUPS
               WHERE (ESTADO = 1) AND (DIA<= :0)
               GROUP BY GRUPO_CMS, AVAYA_ASESOR) AGROUPS INNER JOIN
               AVAYA ON AGROUPS.AVAYA_ASESOR = AVAYA.AVAYA LEFT OUTER JOIN
               GRUPOS ON AGROUPS.GRUPO_CMS = GRUPOS.ID
               WHERE (GRUPOS.AUTOMATICO = 1) AND (GRUPOS.LEGAJO_REFERENTE > 0)",fecha);

            foreach (var empleado in gruposAutomaticos)
                InsertEmpleadoIfNecessary((int)empleado.LEGAJO_REFERENTE, (int)empleado.LEGAJO, fecha);
            
        }

        private void InsertEmpleadoIfNecessary(int super, int legajo, DateTime fecha)
        {
            var inicio = (DateTime.Today.AddDays(5));
            var fin = (inicio.AddDays(30));
            DateTime fechaDotacion = inicio;


            var record = _dotacion.Get(legajo: legajo, IDSuperior: super, Dia: inicio);
            if (record != null)
            {
                if (record.USUARIO_ALTA != "IMPORTADOR") return;
                _dotacion.Delete(record);
            }

            var supervisor = _sap.Get(Legajo: super);
            var empleado = _sap.Get(Legajo: legajo);
 
            if (empleado==null) return;
            if (supervisor==null) return;

            double hh = ((double.Parse(empleado.HORAS_REALES.ToString(), CultureInfo.InvariantCulture))/5)*3600;
                

            dynamic registros = new List<ExpandoObject>();
           
               
            
            while (fechaDotacion <=fin)
            {
                var current = _dotacion.Get(Legajo: legajo, Dia: fechaDotacion);                
                if (current == null)
                {       
                    dynamic registro = new ExpandoObject();                   
                    registro.ID = DateTime.Now.Ticks;
                    registro.LEGAJO = legajo;
                    registro.NOMBRE = (string)empleado.NOMBRE;
                    registro.APELLIDO = (string)empleado.APELLIDO;
                    registro.TIPO_EMPLEADO = empleado.POSICION.ToString().Length > 30 ? empleado.POSICION.ToString().Trim().Substring(0, 30) : empleado.POSICION.ToString();
                    registro.SITE_REAL = (string)empleado.DIRECCION_UBI;
                    registro.HCONTRATO = hh;
                    registro.TIPOSUPERIOR = supervisor.POSICION.ToString().Length > 20 ? supervisor.POSICION.ToString().Trim().Substring(0, 20) : supervisor.POSICION.ToString();
                    registro.SUPERIOR = string.Format("{0} {1}", supervisor.APELLIDO, supervisor.NOMBRE);
                    registro.IDSUPERIOR = int.Parse(supervisor.LEGAJO.ToString());
                    registro.ESTADO = 1;
                    registro.USUARIO_ALTA = "IMPORTADOR";
                    registro.FECHA_ALTA = DateTime.Today;
                    registro.APEYNOM = string.Format("{0} {1}", empleado.APELLIDO, empleado.NOMBRE);
                    registro.DIA = fechaDotacion;
                    _dotacion.Insert(registro);
                    }
                
                fechaDotacion = fechaDotacion.AddDays(1);
            }
        }

        private void InsertAgroupsIfNecessary(string fecha,int avaya,string tipo,string nombre)
        {
            var fechaParseada = DateTime.ParseExact(fecha, "yyyyMMdd", CultureInfo.InvariantCulture);
            var grupo=nombre.GenerateSequence();
            var asesor_dia = _cms.Get(DIA: fechaParseada, AVAYA_ASESOR: avaya, GRUPO_CMS: grupo, OrderBy: "AVAYA_ASESOR");

            if (asesor_dia != null) return ;
            dynamic record = new ExpandoObject();

            record.ID = DateTime.Now.Ticks;
            record.DIA = fechaParseada;
            record.ACD_NO = 1;
            record.ESTADO = 1;
            record.ITEM_TYPE = tipo;
            record.FECHA_IMPORTADO = DateTime.Now;
            record.AVAYA_ASESOR = avaya;
            record.GRUPO_CMS = grupo;

            _cms.Insert(record);
        }

        private void InsertGrupoIfNecessary(int id,string nombre)
        {
          if (nombre.StartsWith("V_") || nombre.StartsWith("XX_")) return ;
            try
            {
                var request = WebRequest.Create(string.Format("http://localhost:5500/grupos/{0}", id));
                request.Method = "GET";
                request.GetResponse();                
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null)
                {
                    var resp = (HttpWebResponse)ex.Response;
                    if (resp.StatusCode != HttpStatusCode.NotFound) return;
                    
                    var request = WebRequest.Create("http://localhost:5500/grupos/");
                    string data = string.Format("Nombre={0}", nombre);
                    request.Method = "POST";
                    byte[] byteArray = Encoding.UTF8.GetBytes(data);
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = byteArray.Length;
                    Stream dataStream = request.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();
                    request.GetResponse();                    
                }
            }
        }        
    }
}
