using BlogApi.Context;
using BlogApi.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Data.Repositories.AddressRepository;

public class AddressRepository : IAddressRepository
{
    private readonly GarDbContext _context;

    public AddressRepository(GarDbContext context)
    {
        _context = context;
    }

    public async Task<List<SearchAddressDto>> Search(long parentObjectId, string query)
    {
        var queryLowered = query.ToLower();
            
        var addressObjects = await (
                from asAdmHierarchy in _context.AsAdmHierarchies
                join asAddrObj in _context.AsAddrObjs
                    on asAdmHierarchy.Objectid equals asAddrObj.Objectid
                where asAdmHierarchy.Parentobjid == parentObjectId
                      && asAddrObj.Isactive == 1
                      && asAddrObj.Isactual == 1
                      && asAddrObj.Name.ToLower().Contains(queryLowered)
                select new SearchAddressDto
                {
                    ObjectId = asAddrObj.Objectid,
                    ObjectGuid = asAddrObj.Objectguid,
                    Text = $"{asAddrObj.Typename} {asAddrObj.Name}",
                    ObjectLevel = ObjectLevelDescriptionMap.ObjectDescriptionFromLevel[asAddrObj.Level].ObjectLevel,
                    ObjectLevelText = ObjectLevelDescriptionMap.ObjectDescriptionFromLevel[asAddrObj.Level]
                        .ObjectLevelText
                }
            )
            .Take(20)
            .ToListAsync();

        var houses = await (
                from asAdmHierarchy in _context.AsAdmHierarchies
                join asHouse in _context.AsHouses
                    on asAdmHierarchy.Objectid equals asHouse.Objectid
                where asAdmHierarchy.Parentobjid == parentObjectId
                      && asHouse.Isactive == 1
                      && asHouse.Isactual == 1
                      && asHouse.Housenum.ToLower().Contains(queryLowered)
                select new SearchAddressDto
                {
                    ObjectId = asHouse.Objectid,
                    ObjectGuid = asHouse.Objectguid,
                    Text = asHouse.Housenum,
                    ObjectLevel = ObjectLevelDescriptionMap.ObjectDescriptionFromLevel["10"].ObjectLevel,
                    ObjectLevelText = ObjectLevelDescriptionMap.ObjectDescriptionFromLevel["10"]
                        .ObjectLevelText
                }
            )
            .Take(10)
            .ToListAsync();

        var result = addressObjects.Concat(houses).ToList();
        return result;
    }

    public async Task<List<SearchAddressDto>> Chain(Guid objectGuid)
    {
        throw new NotImplementedException();
    }
}