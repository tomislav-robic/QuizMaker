using AutoMapper;
using QuizMaker.Core.DTOs;
using QuizMaker.Core.Entities;
using System;
using System.Linq;

namespace QuizMaker.API.MappingProfiles
{
    public class QuizMappingProfile : Profile
    {
        public QuizMappingProfile()
        {
            CreateMap<QuizCreateDTO, Quiz>()
                .ForMember(dest => dest.QuizQuestions, opt => opt.Ignore()) 
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.EditedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<Quiz, QuizDetailDTO>()
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.QuizTags.Select(qt =>
                new TagSummaryDTO {
                    Id = qt.TagId,
                    Tag = qt.Tag.Name
                })))
                .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.QuizQuestions.Select(qq =>
                new QuestionSummaryDTO
                {
                    Id = qq.Question.Id,
                    Text = qq.Question.Text,
                    Answer = qq.Question.Answer,
                    CreatedAt = qq.Question.CreatedAt,
                    EditedAt = qq.Question.EditedAt
                })));

            CreateMap<Quiz, QuizSummaryDTO>();

            CreateMap<QuestionCreateDTO, Question>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.EditedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.QuizQuestions, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore()); 

            CreateMap<Question, QuestionDetailDTO>()
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.TagQuestions.Select(tq =>
                new TagSummaryDTO
                {
                    Id = tq.TagId,
                    Tag = tq.Tag.Name
                })))
                .ForMember(dest => dest.RelatedQuizzes, opt => opt.MapFrom(src => src.QuizQuestions.Select(qq =>
                new QuizSummaryDTO
                {
                    Id = qq.Quiz.Id,
                    Name = qq.Quiz.Name,
                    CreatedAt = qq.Quiz.CreatedAt,
                    EditedAt = qq.Quiz.EditedAt
                })));

            CreateMap<Question, QuestionSummaryDTO>();

            CreateMap<QuizCreateDTO, Quiz>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.EditedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Id, opt => opt.Ignore()) 
                .ForMember(dest => dest.QuizQuestions, opt => opt.MapFrom(src => src.Questions.Select(q => new QuizQuestion
                {
                    Question = new Question
                    {
                        Text = q.Text,
                        Answer = q.Answer,
                        CreatedAt = DateTime.UtcNow,
                        EditedAt = DateTime.UtcNow
                    }
                })));
        }
    }
}
