namespace GameServer.Packages;

public class CurrentSession
{
    public TileType[][]? _grid { get; private set; }

    public CurrentSession(string jsonFilePath)
    {
        _grid = MapsReader.GetMap(jsonFilePath);
        PrintGrid();
    }

    private void PrintGrid()
    {
        foreach (var row in _grid)
        {
            foreach (var tile in row)
            {
                Console.Write(tile + " ");
            }
            Console.WriteLine();
        }
    }
}