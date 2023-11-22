using BlogApi.Dtos;

namespace BlogApi.Services.AddressService;

public interface IAddressService
{
    Task<List<SearchAddressDto>> Search(long parentObjectId, string query);
    Task<List<SearchAddressDto>> Chain(Guid objectGuid);
}