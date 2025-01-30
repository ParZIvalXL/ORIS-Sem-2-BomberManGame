using System.Numerics;
using Newtonsoft.Json;

namespace GameServer.Packages;

public class PlayerPackage
{ 
    public string Nickname { get; set; }
    public float Speed  { get; set; }
    public float Health { get; set; }   
    public Vector2 Direction { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public static string Type { get; set; }
}