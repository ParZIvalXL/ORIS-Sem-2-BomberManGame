using System;
using Newtonsoft.Json;
using UnityEngine;

public class PlayerPackage
{
    public string Nickname { get; set; }
    public float Speed  { get; set; }
    public float Health { get; set; }
    public Vector2 Direction { get; set; }
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public string Type { get; set; } = "PlayerPackage";
    public int SpawnPositionX { get; set; }
    public int SpawnPositionY { get; set; }
}