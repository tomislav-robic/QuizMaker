﻿using QuizMaker.Core.Entities;
using QuizMaker.Core.DTOs;
using System.Net;
using System.Net.Http;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using QuizMaker.Core.Interfaces;
using System.Collections.Generic;
using System;
using System.Linq;

namespace QuizMaker.API.Controllers
{
    [RoutePrefix("api/question")]
    public class QuestionController : BaseApiController
    {
        public QuestionController(IUnitOfWork unitOfWork, IMapper mapper, IQuestionService questionService) : base(unitOfWork, mapper, questionService)
        {
        }

        // POST: api/question
        [HttpPost]
        [Route("")]
        public async Task<HttpResponseMessage> CreateQuestionAsync([FromBody] QuestionCreateDTO questionCreateDto)
        {
            if (questionCreateDto == null || string.IsNullOrEmpty(questionCreateDto.Text) || string.IsNullOrEmpty(questionCreateDto.Answer))
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid question data.");

            try
            {
                var question = _mapper.Map<Question>(questionCreateDto);

                if (questionCreateDto.QuizId.HasValue)
                {
                    var quiz = await _quizMakerDb.Quizzes.GetByIdAsync(questionCreateDto.QuizId.Value);
                    if (quiz == null)
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Quiz not found.");
                    if (quiz.DeletedAt != null)
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Quiz is deleted.");

                    question.QuizQuestions = new List<QuizQuestion> { new QuizQuestion { QuizId = quiz.Id, Question = question } };
                }

                _questionService.PrepareQuestion(question);
                _quizMakerDb.Questions.Add(question);
                await _quizMakerDb.CompleteAsync();

                return Request.CreateResponse(HttpStatusCode.Created, _mapper.Map<QuestionDetailDTO>(question));
            }
            catch (DbUpdateException ex)
            {
                LogError(ex);
                var sqlException = ex.GetBaseException() as SqlException;
                if (sqlException != null)
                {
                    switch (sqlException.Number)
                    {
                        case 2627:  // Unique constraint error
                        case 547:   // Constraint check violation
                        case 2601:  // Duplicated key row error
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "A question with the same text already exists.");
                        default:
                            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "A SQL error occurred: " + sqlException.Message);
                    }
                }
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "A database error occurred: " + ex.Message);
            }
            catch (Exception ex)
            {
                // Catch all other errors
                LogError(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "An error occurred: " + ex.Message);
            }
        }

        // GET: api/question/{id}
        [HttpGet]
        [Route("{id:int}")]
        public async Task<HttpResponseMessage> GetQuestionAsync(int id)
        {
            try
            {
                var question = await _quizMakerDb.Questions.GetByIdAsync(id);

                if (question == null)
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Question not found.");

                if (question.DeletedAt.HasValue)
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "This question is deleted.");

                var questionDetailDto = _mapper.Map<QuestionDetailDTO>(question);

                return Request.CreateResponse(HttpStatusCode.OK, questionDetailDto);
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        // DELETE: api/question/{id}
        [HttpDelete]
        [Route("{id:int}")]
        public async Task<HttpResponseMessage> DeleteQuestionAsync(int id)
        {
            try
            {
                var question = await _quizMakerDb.Questions.GetByIdAsync(id);
                if (question == null)
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Question not found.");

                if (question.QuizQuestions.Any())
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Question is linked to one or more quizzes and cannot be deleted.");

                question.DeletedAt = DateTime.UtcNow;

                await _quizMakerDb.CompleteAsync();
                return Request.CreateResponse(HttpStatusCode.OK, "Question deleted successfully.");
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        // POST: api/question/{id}/revive
        [HttpPost]
        [Route("{id:int}/revive")]
        public async Task<HttpResponseMessage> ReviveQuestionAsync(int id)
        {
            try
            {
                var question = await _quizMakerDb.Questions.GetByIdAsync(id);

                if (question == null)
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Question not found.");

                if (question.DeletedAt == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Question is not deleted.");

                question.DeletedAt = null;
                await _quizMakerDb.CompleteAsync();

                return Request.CreateResponse(HttpStatusCode.OK, "Question revived successfully.");
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        // PUT: api/question/{id}
        [HttpPut]
        [Route("{id:int}")]
        public async Task<HttpResponseMessage> EditQuestionAsync(int id, [FromBody] QuestionEditDTO questionEditDto)
        {
            try
            {
                if (questionEditDto == null || string.IsNullOrEmpty(questionEditDto.Text) || string.IsNullOrEmpty(questionEditDto.Answer))
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid question data.");

                var question = await _quizMakerDb.Questions.GetByIdAsync(id);
                if (question == null)
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Question not found.");

                if (question.QuizQuestions.Count > 1)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Question is linked to multiple quizzes and cannot be edited.");

                question.Text = questionEditDto.Text;
                question.Answer = questionEditDto.Answer;
                question.EditedAt = DateTime.UtcNow;

                await _quizMakerDb.CompleteAsync();
                return Request.CreateResponse(HttpStatusCode.OK, "Question updated successfully.");
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        // POST: api/question/{id}/addTags
        [HttpPost]
        [Route("{id:int}/addTags")]
        public async Task<HttpResponseMessage> AddTagsToQuestionAsync(int id, [FromBody] AddTagsDTO addTagsDto)
        {
            try
            {
                if (addTagsDto == null || string.IsNullOrEmpty(addTagsDto.Tags))
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No tags provided.");

                var question = await _quizMakerDb.Questions.GetByIdAsync(id);
                if (question == null)
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Question not found.");

                var tagNames = addTagsDto.Tags.Split(';')
                                              .Select(t => t.Trim())
                                              .Where(t => !string.IsNullOrEmpty(t))
                                              .ToList();

                if (!tagNames.Any())
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No valid tags provided.");

                var existingTags = await _quizMakerDb.Tags.GetExistingTagsAsync(tagNames);
                foreach (var tagName in tagNames)
                {
                    if (existingTags.TryGetValue(tagName, out var tag))
                    {
                        if (!question.TagQuestions.Any(tq => tq.TagId == tag.Id))
                        {
                            question.TagQuestions.Add(new QuestionTag { Question = question, Tag = tag });
                        }
                    }
                    else
                    {
                        var newTag = new Tag { Name = tagName };
                        question.TagQuestions.Add(new QuestionTag { Question = question, Tag = newTag });
                        _quizMakerDb.Tags.Add(newTag);  
                    }
                }

                await _quizMakerDb.CompleteAsync();
                return Request.CreateResponse(HttpStatusCode.OK, $"Tags added successfully to question with Id {id}.");
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        // POST: api/question/removeTag
        [HttpPost]
        [Route("removeTag")]
        public async Task<HttpResponseMessage> RemoveTagFromQuestionAsync([FromBody] TagToQuestionDTO dto)
        {
            if (dto == null || dto.QuestionId == 0 || dto.TagId == 0)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid data.");

            try
            {
                var question = await _quizMakerDb.Questions.GetByIdAsync(dto.QuestionId);
                if (question == null)
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Question not found.");

                var tag = await _quizMakerDb.Tags.GetByIdAsync(dto.TagId);
                if (tag == null)
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Tag not found.");

                var link = question.TagQuestions.FirstOrDefault(tq => tq.TagId == dto.TagId);
                if (link == null)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Tag not linked to this question.");

                question.TagQuestions.Remove(link);
                await _quizMakerDb.CompleteAsync();

                return Request.CreateResponse(HttpStatusCode.OK, "Tag removed from question successfully.");
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "An error occurred: " + ex.Message);
            }
        }

        // POST: api/question/text-search
        [HttpPost]
        [Route("text-search")]
        public async Task<HttpResponseMessage> GetQuestionsByTextAsync([FromBody] TextSearchPaginationDTO dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.SearchText))
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Search text cannot be empty.");

                var questions = await _quizMakerDb.Questions.GetQuestionsByTextAsync(dto.SearchText, dto.ItemsByPage, dto.PageNumber);
                var questionDtos = _mapper.Map<List<QuestionSummaryDTO>>(questions);

                return !questionDtos.Any()
                    ? Request.CreateResponse(HttpStatusCode.OK, "Page is empty.")
                    : Request.CreateResponse(HttpStatusCode.OK, questionDtos);
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        // POST: api/question/modified-sorted
        [HttpPost]
        [Route("modified-sorted")]
        public async Task<HttpResponseMessage> GetQuestionsModifiedSortedAsync([FromBody] SortedPaginationDTO dto)
        {
            try
            {
                if (dto.SortMode != 1 && dto.SortMode != 2)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid sortMode. Use 1 for ascending, 2 for descending.");

                if (dto.ItemsByPage <= 0)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "ItemsByPage must be greater than zero.");
                if (dto.PageNumber <= 0)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "PageNumber must be greater than zero.");

                var questions = await _quizMakerDb.Questions.GetQuestionsModifiedSortedAsync(dto.SortMode, dto.ItemsByPage, dto.PageNumber);

                var questionDtos = _mapper.Map<List<QuestionSummaryDTO>>(questions);

                if (!questionDtos.Any())
                    return Request.CreateResponse(HttpStatusCode.OK, "Page is empty.");

                return Request.CreateResponse(HttpStatusCode.OK, questionDtos);
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        // POST: api/question/byTags
        [HttpPost]
        [Route("byTags")]
        public async Task<HttpResponseMessage> GetQuestionsByTagsAsync([FromBody] TagsPaginationDTO dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.Tags))
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No tags provided.");

                var tagList = dto.Tags.Split(';')
                                      .Select(tag => tag.Trim())
                                      .Where(tag => !string.IsNullOrEmpty(tag))
                                      .ToList();

                if (!tagList.Any())
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No valid tags provided.");

                if (dto.ItemsByPage <= 0)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "ItemsByPage must be greater than zero.");
                if (dto.PageNumber <= 0)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "PageNumber must be greater than zero.");

                var questions = await _quizMakerDb.Tags.GetQuestionsByTagsAsync(tagList, dto.ItemsByPage, dto.PageNumber);

                var questionDtos = _mapper.Map<List<QuestionSummaryDTO>>(questions);

                if (!questionDtos.Any())
                    return Request.CreateResponse(HttpStatusCode.OK, "Page is empty.");

                return Request.CreateResponse(HttpStatusCode.OK, questionDtos);
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
    }
}
