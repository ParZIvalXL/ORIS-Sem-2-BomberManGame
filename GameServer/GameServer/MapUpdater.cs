using static GameServer.TileType;

namespace GameServer.Packages;

public static class MapUpdater
{
    private static Random random = new Random();
    public static async Task SetBomb(TileType[,]? grid, BombPackage? bomb)
    {
        if (grid[bomb.PositionX, bomb.PositionY] == Bomb) return;
        grid[bomb.PositionX, bomb.PositionY] = Bomb;
        
            await Task.Delay(3000);
        Console.WriteLine("Бомба взорвалась!");

        var bombScript = new BombScripts();
        bombScript.ExplodeBomb(grid, bomb.PositionX, bomb.PositionY);
        grid[bomb.PositionX, bomb.PositionY] = E;
    }
    
    
    public static (int, int) SpawnPlayer(TileType[,]? grid)
    {
        List<(int, int)> validPositions = new List<(int, int)>();

        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);
        
        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < cols; y++)
            {
                if (grid[x, y] == E && IsSafe(x, y, grid))
                {
                    validPositions.Add((x, y));
                }
            }
        }

        if (validPositions.Count > 0)
        {
            var (spawnX, spawnY) = validPositions[random.Next(validPositions.Count)];
            grid[spawnX, spawnY] = S;
            
            return (spawnX, spawnY);
        }
        else
        {
            Console.WriteLine("Не удалось найти безопасную позицию для спавна игрока.");
            
            return (-1, -1);
        }
    }

    private static bool IsSafe(int x, int y, TileType[,]? grid)
    {
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);

        for (int dx = -5; dx <= 5; dx++)
        {
            for (int dy = -5; dy <= 5; dy++)
            {
                int newX = x + dx;
                int newY = y + dy;

                if (newX >= 0 && newX < rows && newY >= 0 && newY < cols)
                {
                    if (grid[newX, newY] == Bomb)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }
}