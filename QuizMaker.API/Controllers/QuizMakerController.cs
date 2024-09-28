using System.Web.Http;

namespace QuizMaker.API.Controllers
{
    public class QuizMakerController : ApiController
    {
        public IHttpActionResult Get()
        {
            return Ok("API is working");
        }
    }
}