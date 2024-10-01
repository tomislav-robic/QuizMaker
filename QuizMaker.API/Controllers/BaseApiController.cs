using AutoMapper;
using QuizMaker.Core.Interfaces;
using System.Web.Http;

namespace QuizMaker.API.Controllers
{
    public class BaseApiController : ApiController
    {
        protected readonly IUnitOfWork _quizMakerDb;
        protected readonly IMapper _mapper;
        protected readonly IQuestionService _questionService;

        public BaseApiController(IUnitOfWork unitOfWork, IMapper mapper, IQuestionService questionService)
        {
            _quizMakerDb = unitOfWork;
            _mapper = mapper;
            _questionService = questionService;
        }
    }
}
