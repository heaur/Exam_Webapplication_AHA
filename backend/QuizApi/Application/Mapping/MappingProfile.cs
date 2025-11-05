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
            CreateMap<QuizCreateDto, Quiz>();   // Title, Description matches
            CreateMap<QuizUpdateDto, Quiz>()    // oppdaterer Title/Description/IsPublished hvis du bruker Map ved update
                .ForAllMembers(o => o.Condition((src, dest, srcMember) => srcMember != null));

            // QUESTION
            CreateMap<Question, QuestionReadDto>()
                .ForMember(d => d.Id,        o => o.MapFrom(s => s.QuestionId))
                .ForMember(d => d.QuizSetId, o => o.MapFrom(s => s.QuizId));
            CreateMap<QuestionCreateDto, Question>()
                .ForMember(d => d.QuizId,    o => o.MapFrom(s => s.QuizSetId));
            CreateMap<QuestionUpdateDto, Question>(); // Text

            // OPTION 
            CreateMap<Option, OptionReadDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.OptionID));  // merk: OptionID i domenet
            CreateMap<OptionCreateDto, Option>();   // Text, IsCorrect, QuestionId
            CreateMap<OptionUpdateDto, Option>();   // Text, IsCorrect

            // RESULT
            CreateMap<Result, ResultReadDto>()
                .ForMember(d => d.Id,      o => o.MapFrom(s => s.ResultId))
                .ForMember(d => d.Score,   o => o.MapFrom(s => s.CorrectCount))
                .ForMember(d => d.TakenAt, o => o.MapFrom(s => s.CompletedAt));

            // Du kan mappe CreateDto -> Entity også (correctCount fra Score).
            // TotalQuestions settes fortsatt best i service (du trenger quiz-spørsmålene).
            CreateMap<ResultCreateDto, Result>()
                .ForMember(d => d.CorrectCount,   o => o.MapFrom(s => s.Score))
                .ForMember(d => d.CompletedAt,    o => o.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.TotalQuestions, o => o.Ignore()); // settes i service
        }
    }
}

