using BlogApi.Dtos;

namespace BlogApi.Services.AddressService;

public interface IAddressService
{
    List<SearchAddressDto> Search(long parentObjectId, string query);
    List<SearchAddressDto> Chain(Guid objectGuid);
}