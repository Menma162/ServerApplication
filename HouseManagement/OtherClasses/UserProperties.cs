using System.Security.Claims;

namespace HouseManagement.OtherClasses
{
    public class UserProperties
    {
        public static string? GetRole(ref HttpContext context)
        {
            return context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;
        }
        public static string? GetIdUser(ref HttpContext context)
        {
            return context.User.Claims.FirstOrDefault(x => x.Type == "idUser")?.Value;
        }
        public static string? GetToken(ref HttpContext context)
        {
            return context.User.Claims.FirstOrDefault(x => x.Type == "token")?.Value;
        }
        public static List<int> GetIdFlats(ref HttpContext context)
        {
            var list = new List<int>();
            int count = Convert.ToInt32(context.User.Claims.FirstOrDefault(x => x.Type == "countFlats")?.Value);
            for(int i = 0; i < count; i++)
            {
                list.Add(Convert.ToInt32(context.User.Claims.FirstOrDefault(x => x.Type == "idFlat" + i.ToString())?.Value));
            }
            return list;
        }
        public static List<int> GetIdHouses(ref HttpContext context)
        {
            var list = new List<int>();
            int count = Convert.ToInt32(context.User.Claims.FirstOrDefault(x => x.Type == "countHouses")?.Value);
            for (int i = 0; i < count; i++)
            {
                list.Add(Convert.ToInt32(context.User.Claims.FirstOrDefault(x => x.Type == "idHouse" + i.ToString())?.Value));
            }
            return list;
        }
        public static int? GetIdFlatOwner(ref HttpContext context)
        {
            return Convert.ToInt32(context.User.Claims.FirstOrDefault(x => x.Type == "idFlatOwner")?.Value);
        }
        public static int? GetIdHouseFlatOwner(ref HttpContext context)
        {
            return Convert.ToInt32(context.User.Claims.FirstOrDefault(x => x.Type == "idHouseFlatOwner")?.Value);
        }
    }
}
