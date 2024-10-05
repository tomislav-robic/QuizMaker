using System.Linq;
using System.Web.Http;

namespace QuizMaker.API
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            UnityConfig.RegisterComponents();
            NLogConfigurator.ConfigureNLog();
        }

        protected void Application_BeginRequest()
        {
            if (Request.Headers.AllKeys.Contains("Origin") && Request.HttpMethod == "OPTIONS")
            {
                Response.Headers.Add("Access-Control-Allow-Origin", "*");
                Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, X-Requested-With, Content-Disposition");
                Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition, X-Sourcefiles"); 
                Response.Headers.Add("Access-Control-Max-Age", "1728000");
                Response.StatusCode = 200;
                Response.End();
            }
        }

    }
}
