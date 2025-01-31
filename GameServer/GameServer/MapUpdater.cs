namespace GameServer.Packages;

public class MapUpdater
{
    public void SetBomb(CurrentSession grid, BombPackage bomb)
    {
        if (Enum.TryParse<TileType>(bomb.BombType, out TileType bombTileType))
        {
            grid._grid[bomb.PositionX][bomb.PositionY] = bombTileType;
        }
    }
}