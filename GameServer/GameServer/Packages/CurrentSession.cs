namespace GameServer.Packages;

public class CurrentSession
{
    public TileType[,]? grid { get; set; }
    public string Type { get; set; }
}