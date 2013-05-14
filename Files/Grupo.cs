using FileHelpers;
namespace FileServer.Files
{
    [DelimitedRecord(";")]
    public class Grupo
    {
        public int Id;
        public string Desc;
        public string Nombre;
        public string IdAvaya;        
    }
}
