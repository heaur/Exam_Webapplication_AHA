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
                .ForMember(d => d.Id,        o => o.MapFrom(s => s.QuestionId))
                .ForMember(d => d.QuizSetId, o => o.MapFrom(s => s.QuizId));
            CreateMap<QuestionCreateDto, Question>()
                .ForMember(d => d.QuizId,    o => o.MapFrom(s => s.QuizSetId));
            CreateMap<QuestionUpdateDto, Question>(); // Text

            // OPTION 
            CreateMap<Option, OptionReadDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.OptionID));
            CreateMap<OptionCreateDto, Option>();   // Text, IsCorrect, QuestionId
            CreateMap<OptionUpdateDto, Option>();   // Text, IsCorrect

            // RESULT
            CreateMap<Result, ResultReadDto>()
                .ForMember(d => d.Id,      o => o.MapFrom(s => s.ResultId))
                .ForMember(d => d.Score,   o => o.MapFrom(s => s.CorrectCount))
                .ForMember(d => d.TakenAt, o => o.MapFrom(s => s.CompletedAt));

            // Mapping from ResultCreateDto to Result entity
            CreateMap<ResultCreateDto, Result>()
                .ForMember(d => d.CorrectCount, o => o.MapFrom(s => s.Score))
                .ForMember(d => d.CompletedAt, o => o.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.TotalQuestions, o => o.Ignore()); 
                
            // USER
            CreateMap<User, UserReadDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.UserId)); 

            CreateMap<UserCreateDto, User>()
                .ForMember(d => d.CreatedAt, o => o.MapFrom(_ => DateTime.UtcNow));

            CreateMap<UserUpdateDto, User>();

        }
    }
}

