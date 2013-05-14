using System;
using Kayak;
using FileServer.Infraestructure;
using System.Text;

namespace FileServer
{
    public class SchedulerDelegate : ISchedulerDelegate
    {
        public void OnException(IScheduler scheduler, Exception e)
        {
            var sb = new StringBuilder();
            Logger.AppendInner(sb, e);
            Logger.LogExceptionAndExit("Check errors.log.", e);
        }

        public void OnStop(IScheduler scheduler)
        {            
            Console.WriteLine("Scheduler is stopping.");
        }
    }
}
