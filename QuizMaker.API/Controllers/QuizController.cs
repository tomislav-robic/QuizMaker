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
using System.Threading.Tasks;

namespace QuizMaker.API.Controllers
{
    [RoutePrefix("api/quiz")]
    public class QuizController : BaseApiController
    {
        public QuizController(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper) { }

        // POST: api/quiz
        [HttpPost]
        [Route("")]
        public async Task<HttpResponseMessage> CreateQuizAsync([FromBody] QuizDTO quizCreateDto)
        {
            try
            {
                if (quizCreateDto == null || string.IsNullOrEmpty(quizCreateDto.Name))
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid quiz data.");

                var quiz = _mapper.Map<Quiz>(quizCreateDto);

                await _quizMakerDb.Quizzes.AddAsync(quiz);
                await _quizMakerDb.CompleteAsync();

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
        public async Task<HttpResponseMessage> GetQuizAsync(int id)
        {
            var quiz = await _quizMakerDb.Quizzes.GetByIdAsync(id);

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
        public async Task<HttpResponseMessage> EditQuizAsync(int id, [FromBody] QuizDTO quizEditDto)
        {
            try
            {
                var quiz = await _quizMakerDb.Quizzes.GetByIdAsync(id);

                if (quiz == null)
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Quiz not found.");

                // Samo mijenjamo naziv i postavljamo EditedAt, bez promjene CreatedAt
                quiz.Name = quizEditDto.Name;
                quiz.EditedAt = DateTime.UtcNow;

                await _quizMakerDb.CompleteAsync();

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
        public async Task<HttpResponseMessage> SoftDeleteQuizAsync(int id)
        {
            try
            {
                var quiz = await _quizMakerDb.Quizzes.GetByIdAsync(id);

                if (quiz == null)
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Quiz not found.");

                // Postavljamo DeletedAt umjesto brisanja iz baze
                quiz.DeletedAt = DateTime.UtcNow;
                await _quizMakerDb.CompleteAsync();

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
        public async Task<HttpResponseMessage> ReviveQuizAsync(int id)
        {
            try
            {
                var quiz = await _quizMakerDb.Quizzes.GetByIdAsync(id);

                if (quiz == null)
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Quiz not found.");

                if (quiz.DeletedAt == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Quiz is not deleted.");

                // Uklanjamo DeletedAt da bi kviz bio aktivan
                quiz.DeletedAt = null;
                await _quizMakerDb.CompleteAsync();

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
        public async Task<HttpResponseMessage> AddTagsAsync(int id, [FromBody] AddTagsDTO addTagsDto)
        {
            try
            {
                if (addTagsDto == null || string.IsNullOrEmpty(addTagsDto.Tags))
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No tags provided.");

                var quiz = await _quizMakerDb.Quizzes.GetByIdAsync(id);

                if (quiz == null)
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Quiz not found.");

                // Splitamo tagove po ";" i trimamo praznine
                var tagList = addTagsDto.Tags.Split(';')
                                             .Select(tag => tag.Trim())
                                             .Where(tag => !string.IsNullOrEmpty(tag))
                                             .ToList();

                if (tagList.Count == 0)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No valid tags provided.");

                // Dohvaćamo sve postojeće tagove iz baze koji odgovaraju traženim imenima
                var existingTags = await _quizMakerDb.Tags.GetExistingTagsAsync(tagList);

                // Prolazimo kroz sve tagove iz liste i dodajemo ih kvizu
                foreach (var tagName in tagList)
                {
                    if (existingTags.TryGetValue(tagName, out var existingTag))
                    {
                        // Ako tag postoji, provjeri postoji li već veza s ovim kvizom
                        var isTagLinked = quiz.QuizTags.Any(qt => qt.TagId == existingTag.Id);

                        if (!isTagLinked)
                        {
                            // Ako veza ne postoji, dodajemo novu
                            quiz.QuizTags.Add(new QuizTag { Quiz = quiz, Tag = existingTag });
                        }
                    }
                    else
                    {
                        // Ako tag ne postoji, kreiramo novi
                        var newTag = new Tag { Name = tagName };
                        quiz.QuizTags.Add(new QuizTag { Quiz = quiz, Tag = newTag });
                    }
                }

                await _quizMakerDb.CompleteAsync();

                return Request.CreateResponse(HttpStatusCode.OK, $"Tags added successfully to quiz with Id {id}.");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("name-sorted")]
        public async Task<HttpResponseMessage> GetQuizzesNameSortedAsync([FromBody] SortedPaginationDTO dto)
        {
            try
            {
                // Validacija SortMode vrijednosti
                if (dto.SortMode != 1 && dto.SortMode != 2)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid SortMode. Use 1 for ascending, 2 for descending.");

                // Asinkrono dohvaćamo kvizove iz baze koristeći prilagođenu metodu
                var quizzes = await _quizMakerDb.Quizzes.GetQuizzesNameSortedAsync(dto.SortMode, dto.ItemsByPage, dto.PageNumber);

                // Koristimo AutoMapper za mapiranje na QuizSummaryDTO
                var quizDtos = _mapper.Map<List<QuizSummaryDTO>>(quizzes);

                // Ako nema kvizova na traženoj stranici, vraćamo poruku da je stranica prazna
                if (!quizDtos.Any())
                    return Request.CreateResponse(HttpStatusCode.OK, "Page is empty.");

                // Vraćamo pronađene kvizove kao odgovor
                return Request.CreateResponse(HttpStatusCode.OK, quizDtos);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("modifiedSorted")]
        public async Task<HttpResponseMessage> GetQuizzesModifiedSortedAsync([FromBody] SortedPaginationDTO dto)
        {
            try
            {
                // Validacija SortMode vrijednosti
                if (dto.SortMode != 1 && dto.SortMode != 2)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid sortMode. Use 1 for ascending, 2 for descending.");

                // Asinkrono dohvaćanje kvizova sortirano prema `EditedAt`
                var quizzes = await _quizMakerDb.Quizzes.GetQuizzesModifiedSortedAsync(dto.SortMode, dto.ItemsByPage, dto.PageNumber);

                // Mapiramo kvizove u QuizSummaryDTO objekte
                var quizDtos = _mapper.Map<List<QuizSummaryDTO>>(quizzes);

                // Ako nema kvizova na traženoj stranici, vraćamo poruku da je stranica prazna
                if (!quizDtos.Any())
                    return Request.CreateResponse(HttpStatusCode.OK, "Page is empty.");

                return Request.CreateResponse(HttpStatusCode.OK, quizDtos);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("byTags")]
        public async Task<HttpResponseMessage> GetQuizzesByTagsAsync([FromBody] TagsPaginationDTO dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.Tags))
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No tags provided.");

                // Razdvajamo tagove iz stringa i radimo validaciju
                var tagList = dto.Tags.Split(';')
                                      .Select(tag => tag.Trim())
                                      .Where(tag => !string.IsNullOrEmpty(tag))
                                      .ToList();

                if (tagList.Count == 0)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No valid tags provided.");

                // Asinkrono dohvaćanje kvizova prema tagovima i paginacija
                var quizzes = await _quizMakerDb.Quizzes.GetQuizzesByTagsAsync(tagList, dto.ItemsByPage, dto.PageNumber);

                // Mapiramo rezultate na DTO
                var quizDtos = _mapper.Map<List<QuizSummaryDTO>>(quizzes);

                if (!quizDtos.Any())
                    return Request.CreateResponse(HttpStatusCode.OK, "Page is empty.");

                return Request.CreateResponse(HttpStatusCode.OK, quizDtos);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
    }
}
