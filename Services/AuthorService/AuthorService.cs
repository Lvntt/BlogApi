using BlogApi.Data.Repositories.AuthorRepo;
using BlogApi.Data.Repositories.UserRepo;
using BlogApi.Dtos;
using BlogApi.Mappers;

namespace BlogApi.Services.AuthorService;

public class AuthorService : IAuthorService
{
    private readonly IAuthorRepository _authorRepository;
    private readonly IUserRepository _userRepository;

    public AuthorService(IAuthorRepository authorRepository, IUserRepository userRepository)
    {
        _authorRepository = authorRepository;
        _userRepository = userRepository;
    }

    public async Task<List<AuthorDto>> GetAllAuthors()
    {
        var authors = await _authorRepository.GetAllAuthors();

        var authorDtos = new List<AuthorDto>();
        foreach (var author in authors)
        {
            var user = await _userRepository.GetUserById(author.UserId);

            var authorDto = AuthorMapper.MapToAuthorDto(user!, author);
            
            authorDtos.Add(authorDto);
        }

        return authorDtos;
    }
}