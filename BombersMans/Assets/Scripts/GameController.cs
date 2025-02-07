using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NetCode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameController : MonoBehaviour
{
    public Queue<Action> actions = new Queue<Action>();
    [SerializeField] public Tilemap ActionTilemap;
    public static GameController Instance;
    private string[,] tiles;
    public List<BombScript> Bombs = new List<BombScript>();
    public Grid grid;
    [SerializeField] private GameObject playerPrefab;
    public List<PlayerController> Players = new List<PlayerController>(); 

    public void AddBomb(BombScript bomb)
    {
        Bombs.Add(bomb);
    }

    public BombScript? GetBomb(float x, float y)
    {
        return Bombs.Find(bomb => Mathf.Approximately(bomb.transform.position.x, x) && Mathf.Approximately(bomb.transform.position.y, y));
    }
    
    public PlayerController? GetPlayer(float x, float y)
    {
        return Players.Find(player => Mathf.Approximately(player.transform.position.x, x) && Mathf.Approximately(player.transform.position.y, y));
    }
    
    [CanBeNull] public PlayerController? GetPlayer(string playerName) => Players.Find(player => player.PlayerNickname == playerName);

    public void AddPlayer(PlayerPackage playerPackage)
    {
        if(playerPackage.Nickname == GameClientScript.Instance.playerName) return;
        var player = Instantiate(playerPrefab, new Vector3(playerPackage.PositionX, playerPackage.PositionY),
            Quaternion.identity);
        var playerController = player.GetComponent<PlayerController>();
        playerController.PlayerNickname = playerPackage.Nickname;
        playerController.health = playerPackage.Health;
        Players.Add(playerController);
    }

    public void AddAction(Action action)
    {
        actions.Enqueue(action);
    }
    
    void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        while (actions.Count > 0)
        {
            actions.Dequeue().Invoke();
        }
    }
    
}
