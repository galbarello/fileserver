using System;
using System.Configuration;
using System.IO;
using System.Net;
using FileServer.Processors;
using Gate.Kayak;
using Ionic.Zip;

namespace FileServer.Services
{
    public class Site 
    {
         private readonly static string _monitorPath = ConfigurationManager.AppSettings["path"];
         private readonly static string _backUpPath = ConfigurationManager.AppSettings["backup"];
         private readonly static int _port= int.Parse(ConfigurationManager.AppSettings["port"]);
         private readonly static FileSystemWatcher _watcher = new FileSystemWatcher(_monitorPath);

        public void Start()
        {
            _watcher.EnableRaisingEvents = true;
             
            _watcher.Created += (object sender, FileSystemEventArgs e)
                => ConvertFileInCommand(e.FullPath,e.Name);                        
                    

            var ep = new IPEndPoint(IPAddress.Any, _port);
            Console.WriteLine("Listening on " + ep);
            KayakGate.Start(new SchedulerDelegate(), ep, Startup.Configuration);
        }
        
        public void Stop()
        {
         
        }

        private void ConvertFileInCommand(string file,string name)
        {
            if (file.Contains("CMSROUP")) new GruposProcessor().Process(file);
            if (file.Contains("Dot CC")) new SapProcessor().Process(file);

            CreateBackUp(file,name);            
        }

        private void CreateBackUp(string file,string name)
        {
            using (var zip = new ZipFile())
            {
                zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;                      
                zip.AddFile(file);
                zip.Save(string.Format("{0}\\{1}.zip",_backUpPath,DateTime.Now.Ticks));
            }
            File.Delete(file);
        }
        
    }
}
