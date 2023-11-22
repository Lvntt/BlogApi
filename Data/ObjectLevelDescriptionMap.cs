using BlogApi.Models;
using BlogApi.Models.Types;

namespace BlogApi.Data;

public static class ObjectLevelDescriptionMap
{
    public static Dictionary<string, ObjectLevelDescription> ObjectDescriptionFromLevel { get; } = new();

    static ObjectLevelDescriptionMap()
    {
        ObjectDescriptionFromLevel.Add("1", new ObjectLevelDescription
        {
            ObjectLevel = GarAddressLevel.Region,
            ObjectLevelText = "Субъект РФ"
        });
        ObjectDescriptionFromLevel.Add("2", new ObjectLevelDescription
        {
            ObjectLevel = GarAddressLevel.AdministrativeArea,
            ObjectLevelText = "Административный район"
        });
        ObjectDescriptionFromLevel.Add("3", new ObjectLevelDescription
        {
            ObjectLevel = GarAddressLevel.MunicipalArea,
            ObjectLevelText = "Муниципальный район"
        });
        ObjectDescriptionFromLevel.Add("4", new ObjectLevelDescription
        {
            ObjectLevel = GarAddressLevel.RuralUrbanSettlement,
            ObjectLevelText = "Сельское/городское поселение"
        });
        ObjectDescriptionFromLevel.Add("5", new ObjectLevelDescription
        {
            ObjectLevel = GarAddressLevel.City,
            ObjectLevelText = "Город"
        });
        ObjectDescriptionFromLevel.Add("6", new ObjectLevelDescription
        {
            ObjectLevel = GarAddressLevel.Locality,
            ObjectLevelText = "Населенный пункт"
        });
        ObjectDescriptionFromLevel.Add("7", new ObjectLevelDescription
        {
            ObjectLevel = GarAddressLevel.ElementOfPlanningStructure,
            ObjectLevelText = "Элемент планировочной структуры"
        });
        ObjectDescriptionFromLevel.Add("8", new ObjectLevelDescription
        {
            ObjectLevel = GarAddressLevel.ElementOfRoadNetwork,
            ObjectLevelText = "Элемент улично-дорожной сети"
        });
        ObjectDescriptionFromLevel.Add("9", new ObjectLevelDescription
        {
            ObjectLevel = GarAddressLevel.Land,
            ObjectLevelText = "Земельный участок"
        });
        ObjectDescriptionFromLevel.Add("10", new ObjectLevelDescription
        {
            ObjectLevel = GarAddressLevel.Building,
            ObjectLevelText = "Здание (сооружение)"
        });
        ObjectDescriptionFromLevel.Add("11", new ObjectLevelDescription
        {
            ObjectLevel = GarAddressLevel.Room,
            ObjectLevelText = "Помещение"
        });
        ObjectDescriptionFromLevel.Add("12", new ObjectLevelDescription
        {
            ObjectLevel = GarAddressLevel.RoomInRooms,
            ObjectLevelText = "Помещения в пределах помещения"
        });
        ObjectDescriptionFromLevel.Add("13", new ObjectLevelDescription
        {
            ObjectLevel = GarAddressLevel.AutonomousRegionLevel,
            ObjectLevelText = "Уровень автономного округа"
        });
        ObjectDescriptionFromLevel.Add("14", new ObjectLevelDescription
        {
            ObjectLevel = GarAddressLevel.IntracityLevel,
            ObjectLevelText = "Уровень внутригородской территории"
        });
        ObjectDescriptionFromLevel.Add("15", new ObjectLevelDescription
        {
            ObjectLevel = GarAddressLevel.AdditionalTerritoriesLevel,
            ObjectLevelText = "Уровень дополнительных территорий  РФ"
        });
        ObjectDescriptionFromLevel.Add("16", new ObjectLevelDescription
        {
            ObjectLevel = GarAddressLevel.LevelOfObjectsInAdditionalTerritories,
            ObjectLevelText = "Уровень объектов на дополнительных территориях"
        });
        ObjectDescriptionFromLevel.Add("17", new ObjectLevelDescription
        {
            ObjectLevel = GarAddressLevel.CarPlace,
            ObjectLevelText = "Машиноместо"
        });
    }
}