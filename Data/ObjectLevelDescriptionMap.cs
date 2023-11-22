using BlogApi.Models;
using BlogApi.Models.Types;

namespace BlogApi.Data;

public static class ObjectLevelDescriptionMap
{
    public static Dictionary<string, ObjectLevelDescription> ObjectDescriptionFromLevel { get; } =
        new()
        {
            {
                "1", new ObjectLevelDescription
                {
                    ObjectLevel = GarAddressLevel.Region,
                    ObjectLevelText = "Субъект РФ"
                }
            },
            {
                "2", new ObjectLevelDescription
                {
                    ObjectLevel = GarAddressLevel.AdministrativeArea,
                    ObjectLevelText = "Административный район"
                }
            },
            {
                "3", new ObjectLevelDescription
                {
                    ObjectLevel = GarAddressLevel.MunicipalArea,
                    ObjectLevelText = "Муниципальный район"
                }
            },
            {
                "4", new ObjectLevelDescription
                {
                    ObjectLevel = GarAddressLevel.RuralUrbanSettlement,
                    ObjectLevelText = "Сельское/городское поселение"
                }
            },
            {
                "5", new ObjectLevelDescription
                {
                    ObjectLevel = GarAddressLevel.City,
                    ObjectLevelText = "Город"
                }
            },
            {
                "6", new ObjectLevelDescription
                {
                    ObjectLevel = GarAddressLevel.Locality,
                    ObjectLevelText = "Населенный пункт"
                }
            },
            {
                "7", new ObjectLevelDescription
                {
                    ObjectLevel = GarAddressLevel.ElementOfPlanningStructure,
                    ObjectLevelText = "Элемент планировочной структуры"
                }
            },
            {
                "8", new ObjectLevelDescription
                {
                    ObjectLevel = GarAddressLevel.ElementOfRoadNetwork,
                    ObjectLevelText = "Элемент улично-дорожной сети"
                }
            },
            {
                "9", new ObjectLevelDescription
                {
                    ObjectLevel = GarAddressLevel.Land,
                    ObjectLevelText = "Земельный участок"
                }
            },
            {
                "10", new ObjectLevelDescription
                {
                    ObjectLevel = GarAddressLevel.Building,
                    ObjectLevelText = "Здание (сооружение)"
                }
            },
            {
                "11", new ObjectLevelDescription
                {
                    ObjectLevel = GarAddressLevel.Room,
                    ObjectLevelText = "Помещение"
                }
            },
            {
                "12", new ObjectLevelDescription
                {
                    ObjectLevel = GarAddressLevel.RoomInRooms,
                    ObjectLevelText = "Помещения в пределах помещения"
                }
            },
            {
                "13", new ObjectLevelDescription
                {
                    ObjectLevel = GarAddressLevel.AutonomousRegionLevel,
                    ObjectLevelText = "Уровень автономного округа"
                }
            },
            {
                "14", new ObjectLevelDescription
                {
                    ObjectLevel = GarAddressLevel.IntracityLevel,
                    ObjectLevelText = "Уровень внутригородской территории"
                }
            },
            {
                "15", new ObjectLevelDescription
                {
                    ObjectLevel = GarAddressLevel.AdditionalTerritoriesLevel,
                    ObjectLevelText = "Уровень дополнительных территорий  РФ"
                }
            },
            {
                "16", new ObjectLevelDescription
                {
                    ObjectLevel = GarAddressLevel.LevelOfObjectsInAdditionalTerritories,
                    ObjectLevelText = "Уровень объектов на дополнительных территориях"
                }
            },
            {
                "17", new ObjectLevelDescription
                {
                    ObjectLevel = GarAddressLevel.CarPlace,
                    ObjectLevelText = "Машиноместо"
                }
            }
        };
}