using System.Xml.XPath;
using BlogApi.Context;
using BlogApi.Dtos;
using BlogApi.Migrations;
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
        var result = new List<SearchAddressDto>();
        
        var addressPath = await (
            from asHouse in _context.AsHouses
            join asAdmHierarchy in _context.AsAdmHierarchies
                on asHouse.Objectid equals asAdmHierarchy.Objectid
            where asHouse.Isactive == 1
                  && asHouse.Isactual == 1
                  && asHouse.Objectguid == objectGuid
            select new string(asAdmHierarchy.Path)
        ).FirstOrDefaultAsync() ?? await (
            from asAddrObj in _context.AsAddrObjs
            join asAdmHierarchy in _context.AsAdmHierarchies
                on asAddrObj.Objectid equals asAdmHierarchy.Objectid
            where asAddrObj.Isactive == 1
                  && asAddrObj.Isactual == 1
                  && asAddrObj.Objectguid == objectGuid
            select new string(asAdmHierarchy.Path)
        ).FirstOrDefaultAsync() ?? throw new KeyNotFoundException($"Could not find object with ObjectGuid={objectGuid}.");

        var parentObjectIds = addressPath.Split(".").Select(e => Convert.ToInt64(e)).ToList();
        foreach (var parentObjectId in parentObjectIds)
        {
            var parentObject = await (
                from asAddrObj in _context.AsAddrObjs
                where asAddrObj.Objectid == parentObjectId
                select new SearchAddressDto
                {
                    ObjectId = asAddrObj.Objectid,
                    ObjectGuid = asAddrObj.Objectguid,
                    Text = $"{asAddrObj.Typename} {asAddrObj.Name}",
                    ObjectLevel = ObjectLevelDescriptionMap.ObjectDescriptionFromLevel[asAddrObj.Level].ObjectLevel,
                    ObjectLevelText = ObjectLevelDescriptionMap.ObjectDescriptionFromLevel[asAddrObj.Level]
                        .ObjectLevelText
                }
            ).FirstOrDefaultAsync();
            
            if (parentObject != null)
            {
                result.Add(parentObject);
            }
        }

        if (parentObjectIds.Count == result.Count) return result;
        
        var house = await (
            from asHouse in _context.AsHouses
            where asHouse.Objectid == parentObjectIds.Last()
            select new SearchAddressDto
            {
                ObjectId = asHouse.Objectid,
                ObjectGuid = asHouse.Objectguid,
                Text = asHouse.Housenum,
                ObjectLevel = ObjectLevelDescriptionMap.ObjectDescriptionFromLevel["10"].ObjectLevel,
                ObjectLevelText = ObjectLevelDescriptionMap.ObjectDescriptionFromLevel["10"]
                    .ObjectLevelText
            }
        ).FirstOrDefaultAsync();
            
        if (house != null)
        {
            result.Add(house);
        }

        return result;
    }
}