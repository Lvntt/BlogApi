using BlogApi.Data;
using BlogApi.Data.DbContext;
using BlogApi.Dtos;
using BlogApi.Exceptions;
using BlogApi.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Services.AddressService;

public class AddressService : IAddressService
{
    private readonly GarDbContext _context;

    public AddressService(GarDbContext context)
    {
        _context = context;
    }

    public async Task<List<SearchAddressDto>> Search(long parentObjectId, string query)
    {
        var queryLowered = query.ToLower();
        
        var addressObjectsQueryable =
            from asAdmHierarchy in _context.AsAdmHierarchies
            join asAddrObj in _context.AsAddrObjs
                on asAdmHierarchy.Objectid equals asAddrObj.Objectid
            where asAdmHierarchy.Parentobjid == parentObjectId
                  && asAddrObj.Isactive == 1
                  && asAddrObj.Isactual == 1
                  && asAddrObj.Lowercasename.Contains(queryLowered)
            // TODO orderby EF.Functions.TrigramsSimilarity(queryLowered, asAddrObj.Lowercasename) descending
            select new SearchAddressDto
            {
                ObjectId = asAddrObj.Objectid,
                ObjectGuid = asAddrObj.Objectguid,
                Text = $"{asAddrObj.Typename} {asAddrObj.Name}",
                ObjectLevel = ObjectLevelDescriptionMap.ObjectDescriptionFromLevel[asAddrObj.Level].ObjectLevel,
                ObjectLevelText = ObjectLevelDescriptionMap.ObjectDescriptionFromLevel[asAddrObj.Level]
                    .ObjectLevelText
            };
            
        addressObjectsQueryable = string.IsNullOrEmpty(query) ? addressObjectsQueryable.Take(10) : addressObjectsQueryable;
        var addressObjects = await addressObjectsQueryable.ToListAsync();

        var housesQueryable =
                from asAdmHierarchy in _context.AsAdmHierarchies
                join asHouse in _context.AsHouses
                    on asAdmHierarchy.Objectid equals asHouse.Objectid
                where asAdmHierarchy.Parentobjid == parentObjectId
                      && asHouse.Isactive == 1
                      && asHouse.Isactual == 1
                      && asHouse.Lowercasehousenum.Contains(queryLowered)
                // TODO orderby EF.Functions.TrigramsSimilarity(queryLowered, asHouse.Lowercasehousenum) descending
                select new SearchAddressDto
                {
                    ObjectId = asHouse.Objectid,
                    ObjectGuid = asHouse.Objectguid,
                    Text = asHouse.GetHouseText(),
                    ObjectLevel = ObjectLevelDescriptionMap.ObjectDescriptionFromLevel["10"].ObjectLevel,
                    ObjectLevelText = ObjectLevelDescriptionMap.ObjectDescriptionFromLevel["10"]
                        .ObjectLevelText
                };
            
        housesQueryable = string.IsNullOrEmpty(query) ? housesQueryable.Take(10) : housesQueryable;
        var houses = await housesQueryable.ToListAsync();

        var result = addressObjects
            .Concat(houses)
            .ToList();
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
                )
                .FirstOrDefaultAsync() ?? await (
                    from asAddrObj in _context.AsAddrObjs
                    join asAdmHierarchy in _context.AsAdmHierarchies
                        on asAddrObj.Objectid equals asAdmHierarchy.Objectid
                    where asAddrObj.Isactive == 1
                          && asAddrObj.Isactual == 1
                          && asAddrObj.Objectguid == objectGuid
                    select new string(asAdmHierarchy.Path)
                )
                .FirstOrDefaultAsync() ??
            throw new EntityNotFoundException($"Could not find address object with ObjectGuid={objectGuid}.");

        var parentObjectIds = addressPath
            .Split(".")
            .Select(e => Convert.ToInt64(e)).ToList();
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
                )
                .FirstOrDefaultAsync();

            if (parentObject != null)
                result.Add(parentObject);
        }

        if (parentObjectIds.Count == result.Count) 
            return result;

        var house = await (
                from asHouse in _context.AsHouses
                where asHouse.Objectid == parentObjectIds.Last()
                select new SearchAddressDto
                {
                    ObjectId = asHouse.Objectid,
                    ObjectGuid = asHouse.Objectguid,
                    Text = asHouse.GetHouseText(),
                    ObjectLevel = ObjectLevelDescriptionMap.ObjectDescriptionFromLevel["10"].ObjectLevel,
                    ObjectLevelText = ObjectLevelDescriptionMap.ObjectDescriptionFromLevel["10"]
                        .ObjectLevelText
                }
            )
            .FirstOrDefaultAsync();

        if (house != null)
            result.Add(house);

        return result;
    }
}