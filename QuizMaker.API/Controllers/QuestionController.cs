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
        public QuestionController(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
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

                await _quizMakerDb.Questions.AddAsync(question);
                await _quizMakerDb.CompleteAsync();

                return Request.CreateResponse(HttpStatusCode.Created, _mapper.Map<QuestionDetailDTO>(question));
            }
            catch (DbUpdateException ex)
            {
                // Provjera unutarnje SQL iznimke
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
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "An error occurred: " + ex.Message);
            }
        }

        // GET: api/question/{id}
        [HttpGet]
        [Route("{id:int}")]
        public async Task<HttpResponseMessage> GetQuestionAsync(int id)
        {
            var question = await _quizMakerDb.Questions.GetByIdAsync(id);

            if (question == null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Question not found.");

            var questionDetailDto = _mapper.Map<QuestionDetailDTO>(question);

            return Request.CreateResponse(HttpStatusCode.OK, questionDetailDto);
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
                        await _quizMakerDb.Tags.AddAsync(newTag);  // Add new tag to the context
                    }
                }

                await _quizMakerDb.CompleteAsync();
                return Request.CreateResponse(HttpStatusCode.OK, $"Tags added successfully to question with Id {id}.");
            }
            catch (Exception ex)
            {
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
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "An error occurred: " + ex.Message);
            }
        }
    }
}
