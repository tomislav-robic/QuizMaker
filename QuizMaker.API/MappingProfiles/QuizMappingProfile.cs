using AutoMapper;
using QuizMaker.Core.DTOs;
using QuizMaker.Core.Entities;
using System;

namespace QuizMaker.API.MappingProfiles
{
    public class QuizMappingProfile : Profile
    {
        public QuizMappingProfile()
        {
            CreateMap<QuizCreateDTO, Quiz>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.EditedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Id, opt => opt.Ignore()); // Ignoriramo automatsko mapiranje Id-a
        }
    }
}
