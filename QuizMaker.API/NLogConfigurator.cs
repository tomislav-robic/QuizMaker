using NLog.Config;
using NLog.Targets;
using NLog;
using System.Configuration;
using System.Web;

namespace QuizMaker.API
{
    public class NLogConfigurator
    {
        public static void ConfigureNLog()
        {
            var config = new LoggingConfiguration();

            var logPath = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["LogPath"]);

            var fileTarget = new FileTarget("logfile")
            {
                FileName = $"{logPath}${{shortdate}}.log",
                Layout = "${longdate} | ${uppercase:${level}} | ${logger} | ${message} ${exception:format=tostring}"
            };

            config.AddTarget(fileTarget);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, fileTarget);

            LogManager.Configuration = config;
        }
    }
}