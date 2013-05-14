using FileServer.Model;
using System.Data;
using FileServer.Helpers;
using System.Dynamic;
using System;
using System.Threading;
using System.Linq;

namespace FileServer.Processors
{
    public class SapProcessor
    {
        dynamic _sap = new TablaSap();
        dynamic _avaya = new TablaAvaya();

        public void Process(string file)
        {
            Helpers.Excel.ReturnData(file).AsParallel().ForAll(sap =>
            {
                var record = _sap.Get(legajo: sap.LEGAJO, mes_validez: file.GetMonth(), anio_validez: file.GetYear());
                if (record == null)
                {
                    sap.MES_VALIDEZ = file.GetMonth();
                    sap.ANIO_VALIDEZ = file.GetYear();
                    sap.FECHA_IMPORTACION = DateTime.Today;
                    //sap.ORIGEN = file;
                    _sap.Insert(sap);
                }

                var record_avaya = _avaya.Get(Avaya: sap.LEGAJO);
                if (record_avaya == null)
                {
                    dynamic avaya = new ExpandoObject();
                    avaya.ID = DateTime.Now.Ticks;
                    avaya.LEGAJO = sap.LEGAJO;
                    avaya.ESTADO = 1;
                    avaya.FECHA_ALTA = DateTime.Now;
                    avaya.USUARIO_ALTA = "IMPORTADOR";
                    avaya.AVAYA = sap.LEGAJO;                    
                    _avaya.Insert(avaya);
                }
            });             
            
        }
    }
}
