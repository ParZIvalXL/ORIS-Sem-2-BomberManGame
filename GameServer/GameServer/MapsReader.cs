using GameServer.Packages;
using Newtonsoft.Json;

namespace GameServer;

public class MapsReader
{
    public static TileType[,]? GetMap(string jsonContent)
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
                    var jaggedArray = JsonConvert.DeserializeObject<TileType[][]>(gridJson);
                    
                    int rows = jaggedArray.Length;
                    int cols = jaggedArray[0].Length;
                    TileType[,] grid = new TileType[rows, cols];
                    
                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < cols; j++)
                        {
                            grid[i, j] = jaggedArray[i][j];
                        }
                    }
                    
                    return grid;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error reading JSON: " + ex.Message);
        }
        return null;
    }
    
    public static void PrintMap(TileType[,]? map)
    {
        if (map == null)
        {
            Console.WriteLine("Карта пуста или не загружена.");
            return;
        }

        int rows = map.GetLength(0);
        int cols = map.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                Console.Write($"{map[i, j]} ");
            }
            Console.WriteLine();
        }
    }

}