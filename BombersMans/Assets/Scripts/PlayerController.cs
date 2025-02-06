using System;
using NetCode;
using NetCode.Packages;
using NUnit.Framework;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    private string _playerNickname;
    public static PlayerController Instance; 

    public string PlayerNickname
    {
        get => _playerNickname;

        set
        {
            if (_playerNickname == null) return;
            _playerNickname = value;
        }
    }

    public float speed = 5f;
    public float health = 100f;
    public Rigidbody2D _rb;
    private Vector2 direction = Vector2.zero;
    [SerializeField] public GameObject[] Bombs;
    public bool ControlLocked { get; set ; }
    
    
    public Vector2 GetIntPosition()
    {
        Vector2 curr = transform.position;
        return new Vector2(Mathf.RoundToInt(curr.x), Mathf.RoundToInt(curr.y));
    }

    public void SpawnBomb(int index)
    {
        var spawnPosition = GetIntPosition();
        var bomb = Instantiate(Bombs[index], spawnPosition, Quaternion.identity);
        if (bomb.TryGetComponent<BombScript>(out var bombScript))
        {
            bombScript.name = _playerNickname;
        }
        GameClientScript.Instance.SendBombPackage(spawnPosition);
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        Instance = this;
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
        if (health <= 0)
        {
            OnDeathSequenceEnded();
            // Вызов смерти
        }
        UIManager.Instance.UpdateHealthBar();
    }
    private void OnDeathSequenceEnded()
    {
        gameObject.SetActive(false);
        UIManager.Instance.ShowGameOver("Вы были убиты собственной бомбой... ", PlayerGameEndReason.DeadByHimself);
        // Вызов проверки состояния игры для текущего игрока (проиграл, причина смерти, победа, и т.д.)
    }

    public void SendPlayerPackage()
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
        
        GameClientScript.Instance.SendPlayerPackage(playerPackage);
    }
}

public enum PlayerGameEndReason
{
    Winner,
    DeadByHimself,
    DeadByEnemy,
    Unknown
}