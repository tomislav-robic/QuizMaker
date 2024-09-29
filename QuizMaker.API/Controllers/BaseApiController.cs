using AutoMapper;
using QuizMaker.Core.Interfaces;
using System.Web.Http;

namespace QuizMaker.API.Controllers
{
    public class BaseApiController : ApiController
    {
        protected readonly IUnitOfWork _quizMakerDb;
        protected readonly IMapper _mapper;

        public BaseApiController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _quizMakerDb = unitOfWork;
            _mapper = mapper;
        }
    }
}
