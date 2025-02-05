using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapBuilder : MonoBehaviour
{
    public static MapBuilder Instance { get; private set; }
    public Grid grid;
    public Tilemap main;
    public Tilemap unbreakable;
    public Tilemap breakable;
    public Tile unbreakableTile;
    public Tile breakableTile;
    public Tile floorTile;
    public Tile boundsTile;

    private void Awake()
    {
        Instance = this;
    }

    public void GenerateMap(CurrentSession session)
    {
        ClearMap();

        var mapToBuild = session.grid;

        for (int i = 0; i < mapToBuild.GetLength(0); i++)
        {
            for (int j = 0; j < mapToBuild.GetLength(1); j++)
            {
                var tile = mapToBuild[i, j];
                var tilePosition = new Vector3Int(i, j, 0);
            }
        }
    }

    private void ClearMap()
    {
        main.ClearAllTiles();
        unbreakable.ClearAllTiles();
        breakable.ClearAllTiles();
    }
}