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

    [CanBeNull]
    public BombScript GetBomb(float x, float y)
    {
        return Bombs.Find(bomb => Mathf.Approximately(bomb.transform.position.x, x) && Mathf.Approximately(bomb.transform.position.y, y));
    }

    public void DeletePlayerFromScene(string playerName)
    {
        var player = GetPlayer(playerName);
        if (player == null) return;
        player.gameObject.SetActive(false);
    }
    
    [CanBeNull]
    public PlayerController GetPlayer(float x, float y)
    {
        return Players.Find(player => Mathf.Approximately(player.transform.position.x, x) && Mathf.Approximately(player.transform.position.y, y));
    }
    
    [CanBeNull] 
    public PlayerController GetPlayer(string playerName) => Players.Find(player => player.PlayerNickname.Equals(playerName));

    public void AddPlayer(PlayerPackage playerPackage)
    {
        if(playerPackage.Nickname == GameClientScript.Instance.playerName) return;

        Debug.Log("Spawn player " + playerPackage.Nickname + " at " + playerPackage.PositionX + ", " + playerPackage.PositionY );
        var player = Instantiate(playerPrefab, new Vector3(playerPackage.PositionX, playerPackage.PositionY),
            Quaternion.identity);
        var playerController = player.GetComponent<PlayerController>();
        playerController.PlayerNickname = playerPackage.Nickname;
        playerController.health = playerPackage.Health;
        Players.Add(playerController);
    }

    public void UpdatePlayer(PlayerPackage playerPackage)
    {
        var player = GetPlayer(playerPackage.Nickname);
        if(!player) 
            return;
        Debug.Log("Update player " + playerPackage.Nickname + " at " + playerPackage.PositionX + ", " + playerPackage.PositionY + "Found player: " + player.PlayerNickname);
        player.transform.position = Vector3.Lerp(player.transform.position, new Vector3(playerPackage.PositionX, playerPackage.PositionY, 0), 0.5f);
        player._rb.position = Vector3.Lerp(player.transform.position, new Vector3(playerPackage.PositionX, playerPackage.PositionY, 0), 0.5f);
        player.SetDirection(new Vector2(playerPackage.DirectionX, playerPackage.DirectionY));
        player.health = playerPackage.Health;
        Debug.Log("player health " + player.health + " Server pl health " + playerPackage.Health);
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
