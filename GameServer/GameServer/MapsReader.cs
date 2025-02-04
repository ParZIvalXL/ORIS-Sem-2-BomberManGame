using GameServer.Packages;
using Newtonsoft.Json;

namespace GameServer;

public class MapsReader
{
    public static TileType[][]? GetMap(string jsonContent)
    {
        try
        {
            var res = JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, object>>>>(jsonContent);
            if (res != null && res.ContainsKey("Maps") && res["Maps"].Count > 0)
            {
                var firstMap = res["Maps"][0];
                if (firstMap.ContainsKey("Grid"))
                {
                    string gridJson = firstMap["Grid"].ToString();
                    return JsonConvert.DeserializeObject<TileType[][]>(gridJson);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error reading JSON: " + ex.Message);
        }
        return null;
    }

}
