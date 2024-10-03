using AutoMapper;
using QuizMaker.Core.Interfaces;
using QuizMaker.Data.Contexts;
using QuizMaker.Data;
using System.Web.Http;
using Unity;
using Unity.Lifetime;
using Unity.WebApi;
using UseCases.Services;
using QuizMaker.API.MappingProfiles;
using System.Configuration;
using Unity.Injection;

namespace QuizMaker.API
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
            var container = new UnityContainer();

            // Database tools registartion
            container.RegisterType<QuizMakerContext>(new HierarchicalLifetimeManager());
            container.RegisterType<IUnitOfWork, UnitOfWork>(new HierarchicalLifetimeManager());

            // AutoMapper registration
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<QuizMappingProfile>();
            });
            IMapper mapper = config.CreateMapper();
            container.RegisterInstance(mapper);

            // Services registration
            container.RegisterType<IQuestionService, QuestionService>();
            container.RegisterType<IExportService, ExportService>(new HierarchicalLifetimeManager());

            // Read configuration values
            var adminUsername = ConfigurationManager.AppSettings["AdminUsername"];
            var adminPassword = ConfigurationManager.AppSettings["AdminPassword"];
            var jwtSecret = ConfigurationManager.AppSettings["JwtSecret"];
            var issuer = ConfigurationManager.AppSettings["Issuer"];
            var audience = ConfigurationManager.AppSettings["Audience"];

            // Register AuthService with injected parameters
            container.RegisterType<IAuthService, AuthService>(
                new HierarchicalLifetimeManager(),
                new InjectionConstructor(adminUsername, adminPassword, jwtSecret, issuer, audience)
            );

            // UnityDependencyResolver registration
            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}