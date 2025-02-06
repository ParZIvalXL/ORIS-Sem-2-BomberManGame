using System.Numerics;
using Newtonsoft.Json;

namespace GameServer.Packages;

public class PlayerPackage
{ 
    public string Nickname { get; set; }
    public float Speed  { get; set; }
    public float Health { get; set; }
    public float DirectionX { get; set; }
    public float DirectionY { get; set; }
    public Double PositionX { get; set; }
    public Double PositionY { get; set; }
    public string Type { get; set; }
    public int SpawnPositionX { get; set; }
    public int SpawnPositionY { get; set; }
}