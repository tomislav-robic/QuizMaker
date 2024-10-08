﻿using AutoMapper;
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
        private readonly IExportService _exportService;
        public QuizController(IUnitOfWork unitOfWork, IMapper mapper, IQuestionService questionService, IExportService exportService) : base(unitOfWork, mapper, questionService) 
        {
            _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        }

        // POST: api/quiz
        [HttpPost]
        [Route("")]
        public async Task<HttpResponseMessage> CreateQuizWithQuestionsAsync([FromBody] QuizCreateDTO quizCreateDto)
        {
            if (quizCreateDto == null || string.IsNullOrEmpty(quizCreateDto.Name))
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid quiz data.");

            using (var transaction = _quizMakerDb.BeginTransaction()) 
            {
                try
                {
                    var quiz = _mapper.Map<Quiz>(quizCreateDto);
                    quiz.QuizQuestions.Clear();
                    _quizMakerDb.Quizzes.Add(quiz); 

                    if (quizCreateDto.Questions?.Any() == true)
                    {
                        foreach (var questionCreateDto in quizCreateDto.Questions)
                        {
                            if (string.IsNullOrEmpty(questionCreateDto.Text) || string.IsNullOrEmpty(questionCreateDto.Answer))
                                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid question data.");

                            var question = _mapper.Map<Question>(questionCreateDto);
                            question.QuizQuestions = new List<QuizQuestion>
                            {
                                new QuizQuestion { QuizId = quiz.Id, Question = question } 
                             };

                            _questionService.PrepareQuestion(question);
                            _quizMakerDb.Questions.Add(question); 
                        }
                    }

                    await _quizMakerDb.CompleteAsync();
                    transaction.Commit();

                    var quizDetailDto = _mapper.Map<QuizDetailDTO>(quiz);
                    return Request.CreateResponse(HttpStatusCode.Created, quizDetailDto);
                }
                catch (DbUpdateException ex)
                {
                    LogError(ex);
                    transaction.Rollback(); 
                    var sqlException = ex.GetBaseException() as SqlException;
                    if (sqlException != null)
                    {
                        switch (sqlException.Number)
                        {
                            case 2627:
                            case 547:
                            case 2601:
                                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "A quiz or a question with the same unique value already exists.");
                            default:
                                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "A SQL error occurred: " + sqlException.Message);
                        }
                    }
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "A database error occurred: " + ex.Message);
                }
                catch (Exception ex)
                {
                    LogError(ex);
                    transaction.Rollback(); 
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "An error occurred: " + ex.Message);
                }
            }
        }

        // GET: api/quiz/{id}
        [HttpGet]
        [Route("{id:int}")]
        public async Task<HttpResponseMessage> GetQuizAsync(int id)
        {
            try
            {
                var quiz = await _quizMakerDb.Quizzes.GetByIdAsync(id);

                if (quiz == null)
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Quiz not found.");

                if (quiz.DeletedAt != null)
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "This quiz is deleted.");

                var quizDetailDto = _mapper.Map<QuizDetailDTO>(quiz);

                return Request.CreateResponse(HttpStatusCode.OK, quizDetailDto);
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"An error occurred: {ex.Message}");
            }
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

                quiz.Name = quizEditDto.Name;
                quiz.EditedAt = DateTime.UtcNow;

                await _quizMakerDb.CompleteAsync();

                var quizSummaryDto = _mapper.Map<QuizSummaryDTO>(quiz);

                return Request.CreateResponse(HttpStatusCode.OK, quizSummaryDto);
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        // POST: api/quiz/addQuestion
        [HttpPost]
        [Route("addQuestion")]
        public async Task<HttpResponseMessage> AddQuestionToQuizAsync([FromBody] QuestionToQuizDTO dto)
        {
            if (dto == null || dto.QuizId == 0 || dto.QuestionId == 0)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid data.");

            try
            {
                var quiz = await _quizMakerDb.Quizzes.GetByIdAsync(dto.QuizId);
                if (quiz == null)
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Quiz not found.");

                if (quiz.DeletedAt != null)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Quiz is deleted.");

                var question = await _quizMakerDb.Questions.GetByIdAsync(dto.QuestionId);
                if (question == null)
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Question not found.");

                var existingLink = quiz.QuizQuestions.Any(qq => qq.QuestionId == dto.QuestionId);
                if (existingLink)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Question already linked to this quiz.");

                quiz.QuizQuestions.Add(new QuizQuestion { QuizId = dto.QuizId, QuestionId = dto.QuestionId });
                await _quizMakerDb.CompleteAsync();

                return Request.CreateResponse(HttpStatusCode.OK, "Question added to quiz successfully.");
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "An error occurred: " + ex.Message);
            }
        }

        // POST: api/quiz/removeQuestion
        [HttpPost]
        [Route("removeQuestion")]
        public async Task<HttpResponseMessage> RemoveQuestionFromQuizAsync([FromBody] QuestionToQuizDTO dto)
        {
            if (dto == null || dto.QuizId == 0 || dto.QuestionId == 0)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid data.");

            try
            {
                var quiz = await _quizMakerDb.Quizzes.GetByIdAsync(dto.QuizId);
                if (quiz == null)
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Quiz not found.");

                var question = await _quizMakerDb.Questions.GetByIdAsync(dto.QuestionId);
                if (question == null)
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Question not found.");

                var link = quiz.QuizQuestions.FirstOrDefault(qq => qq.QuestionId == dto.QuestionId);
                if (link == null)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Question not linked to this quiz.");

                quiz.QuizQuestions.Remove(link);
                await _quizMakerDb.CompleteAsync();

                return Request.CreateResponse(HttpStatusCode.OK, "Question removed from quiz successfully.");
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "An error occurred: " + ex.Message);
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
                LogError(ex);
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

                quiz.DeletedAt = null;
                await _quizMakerDb.CompleteAsync();

                return Request.CreateResponse(HttpStatusCode.OK, "Quiz revived successfully.");
            }
            catch (Exception ex)
            {
                LogError(ex);
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

                var tagList = addTagsDto.Tags.Split(';')
                                             .Select(tag => tag.Trim())
                                             .Where(tag => !string.IsNullOrEmpty(tag))
                                             .ToList();

                if (tagList.Count == 0)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No valid tags provided.");

                var existingTags = await _quizMakerDb.Tags.GetExistingTagsAsync(tagList);

                foreach (var tagName in tagList)
                {
                    if (existingTags.TryGetValue(tagName, out var existingTag))
                    {
                        var isTagLinked = quiz.QuizTags.Any(qt => qt.TagId == existingTag.Id);

                        if (!isTagLinked)
                        {
                            quiz.QuizTags.Add(new QuizTag { Quiz = quiz, Tag = existingTag });
                        }
                    }
                    else
                    {
                        var newTag = new Tag { Name = tagName };
                        quiz.QuizTags.Add(new QuizTag { Quiz = quiz, Tag = newTag });
                    }
                }

                await _quizMakerDb.CompleteAsync();

                return Request.CreateResponse(HttpStatusCode.OK, $"Tags added successfully to quiz with Id {id}.");
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        // POST: api/quiz/removeTag
        [HttpPost]
        [Route("removeTag")]
        public async Task<HttpResponseMessage> RemoveTagFromQuizAsync([FromBody] TagToQuizDTO dto)
        {
            if (dto == null || dto.QuizId == 0 || dto.TagId == 0)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid data.");

            try
            {
                var quiz = await _quizMakerDb.Quizzes.GetByIdAsync(dto.QuizId);
                if (quiz == null)
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Quiz not found.");

                var tag = await _quizMakerDb.Tags.GetByIdAsync(dto.TagId);
                if (tag == null)
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Tag not found.");

                var link = quiz.QuizTags.FirstOrDefault(qt => qt.TagId == dto.TagId);
                if (link == null)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Tag not linked to this quiz.");

                quiz.QuizTags.Remove(link);
                await _quizMakerDb.CompleteAsync();

                return Request.CreateResponse(HttpStatusCode.OK, "Tag removed from quiz successfully.");
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "An error occurred: " + ex.Message);
            }
        }

        // POST: api/quiz/name-sorted
        [HttpPost]
        [Route("name-sorted")]
        public async Task<HttpResponseMessage> GetQuizzesNameSortedAsync([FromBody] SortedPaginationDTO dto)
        {
            try
            {
                if (dto.SortMode != 1 && dto.SortMode != 2)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid SortMode. Use 1 for ascending, 2 for descending.");

                var quizzes = await _quizMakerDb.Quizzes.GetQuizzesNameSortedAsync(dto.SortMode, dto.ItemsByPage, dto.PageNumber);

                var quizDtos = _mapper.Map<List<QuizSummaryDTO>>(quizzes);

                if (!quizDtos.Any())
                    return Request.CreateResponse(HttpStatusCode.OK, "Page is empty.");

                return Request.CreateResponse(HttpStatusCode.OK, quizDtos);
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        // POST: api/quiz/modifiedSorted
        [HttpPost]
        [Route("modifiedSorted")]
        public async Task<HttpResponseMessage> GetQuizzesModifiedSortedAsync([FromBody] SortedPaginationDTO dto)
        {
            try
            {
                if (dto.SortMode != 1 && dto.SortMode != 2)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid sortMode. Use 1 for ascending, 2 for descending.");

                var quizzes = await _quizMakerDb.Quizzes.GetQuizzesModifiedSortedAsync(dto.SortMode, dto.ItemsByPage, dto.PageNumber);

                var quizDtos = _mapper.Map<List<QuizSummaryDTO>>(quizzes);

                if (!quizDtos.Any())
                    return Request.CreateResponse(HttpStatusCode.OK, "Page is empty.");

                return Request.CreateResponse(HttpStatusCode.OK, quizDtos);
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        // POST: api/quiz/byTags
        [HttpPost]
        [Route("byTags")]
        public async Task<HttpResponseMessage> GetQuizzesByTagsAsync([FromBody] TagsPaginationDTO dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.Tags))
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No tags provided.");

                var tagList = dto.Tags.Split(';')
                                      .Select(tag => tag.Trim())
                                      .Where(tag => !string.IsNullOrEmpty(tag))
                                      .ToList();

                if (tagList.Count == 0)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No valid tags provided.");

                var quizzes = await _quizMakerDb.Tags.GetQuizzesByTagsAsync(tagList, dto.ItemsByPage, dto.PageNumber);

                var quizDtos = _mapper.Map<List<QuizSummaryDTO>>(quizzes);

                if (!quizDtos.Any())
                    return Request.CreateResponse(HttpStatusCode.OK, "Page is empty.");

                return Request.CreateResponse(HttpStatusCode.OK, quizDtos);
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        // GET: api/quiz/export/formats
        [HttpGet]
        [Route("export/formats")]
        public HttpResponseMessage GetAvailableExportFormats()
        {   try
            {
                var formats = _exportService.Exporters.Select(e => new { e.ExportFormat, e.FileExtension });
                return Request.CreateResponse(HttpStatusCode.OK, formats);
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        // GET: api/quiz/export/{quizId}?format={format}
        [HttpGet]
        [Route("export/{quizId}")]
        public async Task<HttpResponseMessage> ExportQuizAsync(int quizId, string format)
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Export format is required.");

                var quiz = await _quizMakerDb.Quizzes.GetByIdAsync(quizId);
                if (quiz == null || quiz.DeletedAt != null)
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Quiz not found or deleted.");

                var exporter = _exportService.GetExporter(format);
                if (exporter == null)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Unsupported export format.");

                var fileContent = exporter.ExportQuiz(quiz);
                var result = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(fileContent)
                };
                //result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                //{
                //    FileName = $"{quiz.Name}_{DateTime.UtcNow:yyyyMMddHHmmss}{exporter.FileExtension}"
                //};
                result.Content.Headers.Add("Content-Disposition", $"attachment; filename=\"{quiz.Name}_{DateTime.UtcNow:yyyyMMddHHmmss}{exporter.FileExtension}\"");
                result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                return result;
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
    }
}
