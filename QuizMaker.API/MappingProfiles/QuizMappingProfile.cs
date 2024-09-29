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
            CreateMap<QuizDTO, Quiz>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.EditedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Id, opt => opt.Ignore()); // Ignoriramo automatsko mapiranje Id-a

            CreateMap<Quiz, QuizDetailDTO>()
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.QuizTags.Select(qt => qt.Tag.Name).ToList()));

            CreateMap<Quiz, QuizSummaryDTO>();
        }
    }
}
