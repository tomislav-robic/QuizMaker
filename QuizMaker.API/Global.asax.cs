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

namespace QuizMaker.API
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            var container = new UnityContainer();

            // Registracija baza podataka
            container.RegisterType<QuizMakerContext>(new HierarchicalLifetimeManager());
            container.RegisterType<IUnitOfWork, UnitOfWork>(new HierarchicalLifetimeManager());

            // Registracija AutoMappera
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<QuizMappingProfile>();
            });
            IMapper mapper = config.CreateMapper();
            container.RegisterInstance(mapper);

            // Registriraj UnityDependencyResolver
            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}
