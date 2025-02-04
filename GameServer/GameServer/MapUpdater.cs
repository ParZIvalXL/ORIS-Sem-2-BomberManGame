using static GameServer.TileType;

namespace GameServer.Packages;

public class MapUpdater
{
    public void SetBomb(TileType[][] grid ,BombPackage bomb)
    {
        if (grid[bomb.PositionX][bomb.PositionY] != Bomb) return;
        grid[bomb.PositionX][bomb.PositionY] = Bomb;
    }
}