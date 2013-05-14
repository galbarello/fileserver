using System;
using System.Text;
using FileServer.Infraestructure;
using FileServer.Services;

namespace FileServer
{
    class Program
    {
        private static bool _shouldNotStart;
        private static readonly Site _webComponent;
        
        static Program()
        {
            _webComponent = new Site();            
        }

        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0 && (args[0] == "install" || args[0] == "uninstall"))           
                _shouldNotStart = true;                     

            if (!_shouldNotStart)
            {
                try
                {
                    _webComponent.Start();
                }
                catch (Exception e)
                {
                    var sb = new StringBuilder();
                    Logger.AppendInner(sb, e);
                    Logger.LogExceptionAndExit("Couldn't start! Check errors.log.", e);
                }
            }         

            if (_shouldNotStart)  _webComponent.Stop();
            Console.ReadKey();
        } 
    }
}
