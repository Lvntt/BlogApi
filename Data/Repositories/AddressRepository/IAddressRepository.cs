using BlogApi.Dtos;

namespace BlogApi.Data.Repositories.AddressRepository;

public interface IAddressRepository
{
    List<SearchAddressDto> Search(long parentObjectId, string query);
    List<SearchAddressDto> Chain(Guid objectGuid);
}