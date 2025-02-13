﻿using System;
using System.Linq;
using System.Threading.Tasks;
using NetCode;
using NetCode.Packages;
using NUnit.Framework;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    private string _playerNickname;
    //public static PlayerController Instance; 
    private bool timeOut = false;
    public bool isClient = false;

    public string PlayerNickname
    {
        get => _playerNickname;

        set
        {
            if (string.IsNullOrEmpty(value)) return;
            nicknameText.text = value;
            _playerNickname = value;
        }
    }

    public float speed = 5f;
    private float _health = 100f;
    public float health { get => _health; 
        set 
        {
            _health = value;
            Debug.Log("setting health " + value + " for " + _playerNickname + " Server pl name " + GameClientScript.Instance.playerName);
            if(_playerNickname.Equals(GameClientScript.Instance.playerName))
            {
                UIManager.Instance.UpdateHealthBar();
                if (health <= 0)
                {
                    OnDeathSequenceEnded();
                    // Вызов смерти
                }
            }else
            {
                if (health <= 0)
                {
                    GameController.Instance.Players.Remove(this);
                    Destroy(gameObject);
                }
            }
        } 
    }
    public Rigidbody2D _rb;
    private Vector2 direction = Vector2.zero;
    [SerializeField] public GameObject[] Bombs;
    [SerializeField] private TMP_Text nicknameText;
    public bool ControlLocked { get; set ; }
    
    
    public Vector2 GetIntPosition()
    {
        Vector2 curr = transform.position;
        return new Vector2(Mathf.RoundToInt(curr.x), Mathf.RoundToInt(curr.y));
    }

    public void SpawnBomb(int index)
    {
        if(GameController.Instance.Bombs.Count(b => b.playerName == GameClientScript.Instance.playerName) >= 4) return;
        var spawnPosition = GetIntPosition();
        //var bomb = Instantiate(Bombs[index], spawnPosition, Quaternion.identity);
        //if (bomb.TryGetComponent<BombScript>(out var bombScript))
        //{
        //    bombScript.name = _playerNickname;
        //}
        GameClientScript.Instance.SendBombPackage(spawnPosition);
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        /*if(Instance == null || GameClientScript.Instance.playerName == PlayerNickname)
            Instance = this;*/
    }

    private void FixedUpdate()
    {
        Vector2 position = _rb.position;
        Vector2 translation = speed * Time.fixedDeltaTime * direction;

        _rb.MovePosition(position + translation);
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection;
        if(isClient)
            SendPlayerPackage();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Explosion")) {
            // В случае попадания под взрыв
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log("taking damage " + health);
        UIManager.Instance.UpdateHealthBar();
        if (health <= 0)
        {
            OnDeathSequenceEnded();
            // Вызов смерти
        }
    }
    private void OnDeathSequenceEnded()
    {
        gameObject.SetActive(false);
        // Вызов проверки состояния игры для текущего игрока (проиграл, причина смерти, победа, и т.д.)
    }

    public async Task SendPlayerPackage()
    {
        var playerPackage = new PlayerPackage
        {
            Nickname = GameClientScript.Instance.playerName,
            Speed = speed,
            Health = health,
            DirectionX = direction.x,
            DirectionY = direction.y,
            PositionX = transform.position.x,
            PositionY = transform.position.y
        };
        if (!timeOut)
        {
            GameClientScript.Instance.SendPlayerPackage(playerPackage);
            StartTimeOut();
        }
        
    }

    public async Task StartTimeOut()
    {
        timeOut = true;
        await Task.Delay(50);
        timeOut = false;
    }
}

public enum PlayerGameEndReason
{
    Winner,
    DeadByHimself,
    DeadByEnemy,
    Unknown
}