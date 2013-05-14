using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Dynamic;
using System.IO;

namespace FileServer.Helpers
{
    public static class Excel
    {
        
        public static List<dynamic> ReturnData(string file)
        {            
            var con = new OleDbConnection(CreateConnection(file).ToString());
            string qry = string.Format("SELECT * FROM [{0}]",ListSheetInExcel(file)[0].ToString());
            
            var command=new OleDbCommand(qry,con);                       
            con.Open();
            var reader=command.ExecuteReader();
            var result=reader.ToExpandoList();
            con.Close();          
            return result;
        }

        private static List<dynamic> ToExpandoList(this IDataReader rdr)
        {
            var result = new List<dynamic>();
            while (rdr.Read())            
                result.Add(rdr.RecordToExpando());            
            return result;
        }    
      

        private static dynamic RecordToExpando(this IDataReader rdr)
        {
            dynamic e = new ExpandoObject();
            var d = e as IDictionary<string, object>;           
            for (int i = 0; i < rdr.FieldCount; i++)
                d.Add(rdr.GetName(i).Replace(" ","_").ToUpperInvariant().RemoveDiacritics(), DBNull.Value.Equals(rdr[i]) ? null : rdr[i]);
            return e;
        }

        private static List<string> ListSheetInExcel(string file)
        {
            var listSheet = new List<string>();

            using (var conn = new OleDbConnection(CreateConnection(file).ToString()))
            {
                conn.Open();
                var dtSheet = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                
                foreach (DataRow drSheet in dtSheet.Rows)                
                    if (drSheet["TABLE_NAME"].ToString().Contains("$")) 
                        listSheet.Add(drSheet["TABLE_NAME"].ToString());
                
            }
            return listSheet;
        }

        private static OleDbConnectionStringBuilder CreateConnection(string filePath)
        {
            OleDbConnectionStringBuilder sbConnection = new OleDbConnectionStringBuilder();
            var strExtendedProperties = string.Empty;
            sbConnection.DataSource = filePath;
            if (Path.GetExtension(filePath).Equals(".xls"))//97-03 Excel
            {
                sbConnection.Provider = "Microsoft.Jet.OLEDB.4.0";
                strExtendedProperties = "Excel 8.0;HDR=Yes;IMEX=1";//HDR=ColumnHeader,IMEX=InterMixed
            }
            else if (Path.GetExtension(filePath).Equals(".xlsx"))  //2007 Excel
            {
                sbConnection.Provider = "Microsoft.ACE.OLEDB.12.0";
                strExtendedProperties = "Excel 12.0;HDR=Yes;IMEX=1";
            }
            sbConnection.Add("Extended Properties", strExtendedProperties);
            return sbConnection;
        }
    }
}
