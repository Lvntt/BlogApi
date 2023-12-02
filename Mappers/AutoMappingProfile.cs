using AutoMapper;
using BlogApi.Dtos;
using BlogApi.Models;

namespace BlogApi.Mappers;

public class AutoMappingProfile : Profile
{
    public AutoMappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<Tag, TagDto>();
    }
}