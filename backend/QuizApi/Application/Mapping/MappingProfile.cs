using AutoMapper;
using QuizApi.Domain;
using QuizApi.DTOs;

namespace QuizApi.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Entity -> DTO
            CreateMap<Quiz, QuizReadDto>();
            CreateMap<Question, QuestionReadDto>();
            CreateMap<Option, OptionReadDto>();

            // DTO -> Entity
            CreateMap<QuizCreateDto, Quiz>();
            CreateMap<QuestionCreateDto, Question>();
            CreateMap<OptionCreateDto, Option>();
        }
    }
}
