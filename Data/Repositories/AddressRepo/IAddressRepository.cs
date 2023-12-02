using BlogApi.Dtos;

namespace BlogApi.Data.Repositories.AddressRepo;

public interface IAddressRepository
{
    Task<List<SearchAddressDto>> Search(long parentObjectId, string query);
    Task<List<SearchAddressDto>> Chain(Guid objectGuid);
}