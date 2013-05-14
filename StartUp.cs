using Gate;
using Gate.Kayak;
using Nancy.Hosting.Owin;

namespace FileServer
{
    public static class Startup
    {        
        public static void Configuration(IAppBuilder builder)
        {
            builder                
                .RescheduleCallbacks()
                .RunNancy();
        }

        public static IAppBuilder RunNancy(this IAppBuilder builder)
        {
            return builder.Run(Delegates.ToDelegate(new NancyOwinHost().ProcessRequest));
        }
    }


}
