using BlogApi.Dtos;
using BlogApi.Models;

namespace BlogApi.Mappers;

public static class CommunityMapper
{
    public static Community MapToCommunity(CommunityCreateDto communityCreateDto)
    {
        return new Community
        {
            Id = Guid.NewGuid(),
            CreateTime = DateTime.UtcNow,
            Name = communityCreateDto.Name,
            Description = communityCreateDto.Description,
            IsClosed = communityCreateDto.IsClosed,
            SubscribersCount = 0
        };
    }
    
    public static CommunityFullDto MapToCommunityFullDto(Community community, List<UserDto> administrators)
    {
        return new CommunityFullDto
        {
            Id = community.Id,
            CreateTime = community.CreateTime,
            Name = community.Name,
            Description = community.Description,
            IsClosed = community.IsClosed,
            SubscribersCount = community.SubscribersCount,
            Administrators = administrators
        };
    }
}