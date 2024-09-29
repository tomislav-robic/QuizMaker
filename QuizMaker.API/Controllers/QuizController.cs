using AutoMapper;
using QuizMaker.Core.DTOs;
using QuizMaker.Core.Entities;
using QuizMaker.Core.Interfaces;
using System;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace QuizMaker.API.Controllers
{
    [RoutePrefix("api/quiz")]
    public class QuizController : BaseApiController
    {
        public QuizController(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper) { }

        // POST: api/quiz
        [HttpPost]
        [Route("")]
        public HttpResponseMessage CreateQuiz([FromBody] QuizCreateDTO quizCreateDto)
        {
            try
            {
                if (quizCreateDto == null || string.IsNullOrEmpty(quizCreateDto.Name))
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid quiz data.");

                var quiz = _mapper.Map<Quiz>(quizCreateDto);

                _quizMakerDb.Quizzes.Add(quiz);
                _quizMakerDb.Complete();

                return Request.CreateResponse(HttpStatusCode.Created, quiz);
            }
            catch (DbUpdateException ex)
            {
                // Provjera unutarnje SQL iznimke
                var sqlException = ex.GetBaseException() as SqlException;

                if (sqlException != null)
                {
                    switch (sqlException.Number)
                    {
                        case 2627: // Unique constraint error
                        case 547:  // Constraint check violation
                        case 2601: // Duplicated key row error
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "A quiz with the same name already exists.");
                        default:
                            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "A SQL error occurred: " + sqlException.Message);
                    }
                }
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "A database error occurred: " + ex.Message);
            }
            catch (Exception ex)
            {
                // Catch all other errors
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "An error occurred: " + ex.Message);
            }
        }

        // GET: api/quiz/{id}
        [HttpGet]
        [Route("{id:int}")]
        public HttpResponseMessage GetQuiz(int id)
        {
            var quiz = _quizMakerDb.Quizzes.GetById(id);

            if (quiz == null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Quiz not found.");

            return Request.CreateResponse(HttpStatusCode.OK, quiz);
        }
    }
}
