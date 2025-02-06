using System;
using Newtonsoft.Json;
using UnityEngine;

public class PlayerPackage
{
    public string Nickname { get; set; }
    public float Speed  { get; set; }
    public float Health { get; set; }
    public float DirectionX { get; set; }
    public float DirectionY { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public string Type { get; set; } = "PlayerPackage";
    public int SpawnPositionX { get; set; }
    public int SpawnPositionY { get; set; }
}