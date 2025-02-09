using System;
using System.Linq;
using NetCode;
using NetCode.Packages;
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
    public BoxCollider2D CameraBorder;
    public TileType[,] mapMemory;

    private void Awake()
    {
        Instance = this;
    }

    public void GenerateMap(CurrentSession session = null)
    {
        ClearMap();
        TileType[,] mapToBuild = null;
        mapToBuild = session.grid ?? mapMemory;
        var mapX = mapToBuild.GetLength(0);
        var mapY = mapToBuild.GetLength(1);
        Debug.Log("X: " +  mapX + " Y: " + mapY);
        CameraBorder.transform.position = new Vector3(mapX / 2f - 0.5f, mapY / 2f - 0.5f, 0);
        var bounds = CameraBorder.bounds;
        bounds.size = new Vector3(mapX, mapY, 0);

        for (int i = 0; i < mapX; i++)
        {
            for (int j = 0; j < mapY; j++)
            {
                var tile = mapToBuild[i, j];
                var tilePosition = new Vector3Int(i, j, 0);
                switch (tile)
                {
                    case TileType.B:
                        breakable.SetTile(tilePosition, breakableTile);
                        break;
                    case TileType.D:
                        unbreakable.SetTile(tilePosition, unbreakableTile);
                        break;
                }
                main.SetTile(tilePosition, floorTile);
            }
        }

        for (int i = -1; i < mapX + 1; i++)
        {
            unbreakable.SetTile(new Vector3Int(i, -1, 0), boundsTile);
            unbreakable.SetTile(new Vector3Int(i, mapY, 0), boundsTile);
        }
        
        for (int j = 0; j < mapY; j++)
        {
            unbreakable.SetTile(new Vector3Int(-1, j, 0), boundsTile);
            unbreakable.SetTile(new Vector3Int(mapX,  j, 0), boundsTile);
        }
    }

    public void CreatePlayer(PlayerPackage playerPackage)
    {
        Debug.Log("is player - client? " + playerPackage.Nickname == GameClientScript.Instance.name);
        Debug.Log("got player with nickname: " + playerPackage.Nickname + " when client is: " + GameClientScript.Instance.playerName);
        if (playerPackage.Nickname == GameClientScript.Instance.playerName)
        {
            Debug.Log("Teleporting player to " + playerPackage.SpawnPositionX + ", " + playerPackage.SpawnPositionY);
            var spawnPoint = new Vector3(playerPackage.SpawnPositionX + 1f, playerPackage.SpawnPositionY + 1f, 0);
            var clPlayer = GameClientScript.Instance.clientPlayer;
            clPlayer._rb.position = spawnPoint;
            clPlayer.PlayerNickname = playerPackage.Nickname;
            clPlayer.transform.position = spawnPoint;
            if(GameController.Instance.Players.All(f => f.PlayerNickname != clPlayer.PlayerNickname))
                GameController.Instance.Players.Add(clPlayer);
        }
    }
    private void ClearMap()
    {
        main.ClearAllTiles();
        unbreakable.ClearAllTiles();
        breakable.ClearAllTiles();
    }
}