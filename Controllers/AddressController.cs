using BlogApi.Dtos;
using BlogApi.Services.AddressService;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Controllers;

[Route("api/address")]
[ApiController]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;

    public AddressController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    [HttpGet("search")]
    public ActionResult<List<SearchAddressDto>> Search(long? parentObjectId, string? query)
    {
        return Ok(_addressService.Search(parentObjectId ?? 0, query ?? ""));
    }
}