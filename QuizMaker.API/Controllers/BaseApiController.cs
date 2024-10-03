using AutoMapper;
using NLog;
using QuizMaker.Core.Interfaces;
using System;
using System.Runtime.CompilerServices;
using System.Web.Http;

namespace QuizMaker.API.Controllers
{
    [Authorize]
    public class BaseApiController : ApiController
    {
        protected readonly IUnitOfWork _quizMakerDb;
        protected readonly IMapper _mapper;
        protected readonly IQuestionService _questionService;
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public BaseApiController(IUnitOfWork unitOfWork, IMapper mapper, IQuestionService questionService)
        {
            _quizMakerDb = unitOfWork;
            _mapper = mapper;
            _questionService = questionService;
        }

        protected void LogError(Exception ex, [CallerMemberName] string methodName = "")
        {
            string controllerName = this.GetType().Name;
            Logger.Error(ex, $"Unhandled exception in {controllerName}.{methodName}: {ex.Message}");
        }
    }
}
