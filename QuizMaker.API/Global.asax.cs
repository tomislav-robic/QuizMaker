using AutoMapper;
using QuizMaker.API.MappingProfiles;
using QuizMaker.Core.Interfaces;
using QuizMaker.Data;
using QuizMaker.Data.Contexts;
using System.Data.SqlClient;
using System.Web.Http;
using Unity;
using Unity.Lifetime;
using Unity.WebApi;
using UseCases.Services;

namespace QuizMaker.API
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            UnityConfig.RegisterComponents();
        }
    }
}
