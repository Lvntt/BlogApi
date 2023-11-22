using BlogApi.Data.Repositories.AddressRepository;
using BlogApi.Dtos;

namespace BlogApi.Services.AddressService;

public class AddressService : IAddressService
{
    private readonly IAddressRepository _addressRepository;

    public AddressService(IAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    public List<SearchAddressDto> Search(long parentObjectId, string query)
    {
        return _addressRepository.Search(parentObjectId, query);
    }

    public List<SearchAddressDto> Chain(Guid objectGuid)
    {
        throw new NotImplementedException();
    }
}