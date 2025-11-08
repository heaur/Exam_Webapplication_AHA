using System;
using AutoMapper;
using QuizApi.Domain;
using QuizApi.DTOs;

namespace QuizApi.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // QUIZ
            CreateMap<Quiz, QuizReadDto>()
                .ForMember(d => d.Id,            o => o.MapFrom(s => s.QuizId))
                .ForMember(d => d.QuestionCount, o => o.MapFrom(s => s.Questions == null ? (int?)null : s.Questions.Count));
            CreateMap<QuizCreateDto, Quiz>();   
            CreateMap<QuizUpdateDto, Quiz>()    
                .ForAllMembers(o => o.Condition((src, dest, srcMember) => srcMember != null));


            // QUESTION
            CreateMap<Question, QuestionReadDto>()
                .ForMember(d => d.QuestionId, o => o.MapFrom(s => s.QuestionId))
                .ForMember(d => d.QuizId,     o => o.MapFrom(s => s.QuizId));

            CreateMap<QuestionCreateDto, Question>()
                .ForMember(d => d.QuizId, o => o.MapFrom(s => s.QuizId));
            CreateMap<QuestionUpdateDto, Question>(); // Text


            // OPTION 
            CreateMap<Option, OptionReadDto>()
                .ForMember(d => d.OptionId, o => o.MapFrom(s => s.OptionID))
                .ForMember(d => d.Text,     o => o.MapFrom(s => s.Text))
                .ForMember(d => d.IsCorrect,o => o.MapFrom(s => s.IsCorrect))
                .ForMember(d => d.QuestionId, o => o.MapFrom(s => s.QuestionId));

            CreateMap<OptionCreateDto, Option>();   // Text, IsCorrect, QuestionId
            CreateMap<OptionUpdateDto, Option>();   // Text, IsCorrect


            // RESULT
            CreateMap<Result, ResultReadDto>()
                .ForMember(d => d.ResultId,       o => o.MapFrom(s => s.ResultId))
                .ForMember(d => d.CorrectCount,   o => o.MapFrom(s => s.CorrectCount))
                .ForMember(d => d.TotalQuestions, o => o.MapFrom(s => s.TotalQuestions))
                .ForMember(d => d.CompletedAt,    o => o.MapFrom(s => s.CompletedAt))
                .ForMember(d => d.Percentage,     o => o.MapFrom(s => s.Percentage))
                ;

            // Mapping from ResultCreateDto to Result entity
            CreateMap<ResultCreateDto, Result>()
                .ForMember(d => d.CorrectCount,   o => o.MapFrom(s => s.CorrectCount))
                .ForMember(d => d.TotalQuestions, o => o.MapFrom(s => s.TotalQuestions))
                .ForMember(d => d.CompletedAt,    o => o.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.ResultId,       o => o.Ignore());

        }
    }
}
