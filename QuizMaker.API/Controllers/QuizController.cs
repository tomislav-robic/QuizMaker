using AutoMapper;
using QuizMaker.Core.DTOs;
using QuizMaker.Core.Entities;
using QuizMaker.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Linq;

namespace QuizMaker.API.Controllers
{
    [RoutePrefix("api/quiz")]
    public class QuizController : BaseApiController
    {
        public QuizController(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper) { }

        // POST: api/quiz
        [HttpPost]
        [Route("")]
        public HttpResponseMessage CreateQuiz([FromBody] QuizDTO quizCreateDto)
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
            
            if (quiz.DeletedAt != null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "This quiz is deleted.");

            var quizDetailDto = _mapper.Map<QuizDetailDTO>(quiz);

            return Request.CreateResponse(HttpStatusCode.OK, quizDetailDto);
        }

        // PUT: api/quiz/{id}
        [HttpPut]
        [Route("{id:int}")]
        public HttpResponseMessage EditQuiz(int id, [FromBody] QuizDTO quizEditDto)
        {
            try
            {
                var quiz = _quizMakerDb.Quizzes.GetById(id);

                if (quiz == null)
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Quiz not found.");

                // Samo mijenjamo naziv i postavljamo EditedAt, bez promjene CreatedAt
                quiz.Name = quizEditDto.Name;
                quiz.EditedAt = DateTime.UtcNow;

                _quizMakerDb.Complete();

                return Request.CreateResponse(HttpStatusCode.OK, quiz);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        // DELETE: api/quiz/{id}
        [HttpDelete]
        [Route("{id:int}")]
        public HttpResponseMessage SoftDeleteQuiz(int id)
        {
            try
            {
                var quiz = _quizMakerDb.Quizzes.GetById(id);

                if (quiz == null)
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Quiz not found.");

                // Postavljamo DeletedAt umjesto brisanja iz baze
                quiz.DeletedAt = DateTime.UtcNow;
                _quizMakerDb.Complete();

                return Request.CreateResponse(HttpStatusCode.OK, "Quiz soft-deleted successfully.");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        // POST: api/quiz/{id}/revive
        [HttpPost]
        [Route("{id:int}/revive")]
        public HttpResponseMessage ReviveQuiz(int id)
        {
            try
            {
                var quiz = _quizMakerDb.Quizzes.GetById(id);

                if (quiz == null)
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Quiz not found.");

                if (quiz.DeletedAt == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Quiz is not deleted.");

                // Uklanjamo DeletedAt da bi kviz bio aktivan
                quiz.DeletedAt = null;
                _quizMakerDb.Complete();

                return Request.CreateResponse(HttpStatusCode.OK, "Quiz revived successfully.");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        // POST: api/quiz/{id}/addTags
        [HttpPost]
        [Route("{id:int}/addTags")]
        public HttpResponseMessage AddTags(int id, [FromBody] List<string> tags)
        {
            try
            {
                var quiz = _quizMakerDb.Quizzes.GetById(id);

                if (quiz == null)
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Quiz not found.");

                if (tags == null || !tags.Any())
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No tags provided.");

                // Splitamo tagove iz liste i dodajemo ih kvizu
                foreach (var tagName in tags.Select(tag => tag.Trim()).Where(tag => !string.IsNullOrEmpty(tag)))
                {
                    var tag = _quizMakerDb.Tags.FirstOrDefault(t => t.Name == tagName) ?? new Tag { Name = tagName };
                    quiz.QuizTags.Add(new QuizTag { Quiz = quiz, Tag = tag });
                }

                _quizMakerDb.Complete();

                return Request.CreateResponse(HttpStatusCode.OK, $"Tags added successfully to quiz with Id {id}.");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
    }
}
