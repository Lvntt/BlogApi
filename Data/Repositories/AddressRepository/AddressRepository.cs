using BlogApi.Context;
using BlogApi.Dtos;
using BlogApi.Migrations;

namespace BlogApi.Data.Repositories.AddressRepository;

public class AddressRepository : IAddressRepository
{
    private readonly GarDbContext _context;

    public AddressRepository(GarDbContext context)
    {
        _context = context;
    }

    public List<SearchAddressDto> Search(long parentObjectId, string query)
    {
        throw new NotImplementedException();
    }

    public List<SearchAddressDto> Chain(Guid objectGuid)
    {
        throw new NotImplementedException();
    }
}