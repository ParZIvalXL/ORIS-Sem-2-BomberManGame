using System;
using System.Collections;
using System.Linq;
using NetCode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class BombScript : MonoBehaviour
{
    public float gameTick;
    public BombType type = BombType.Classic; 
    public string playerName;
    public PlayerController player;
    public Vector2[] directions = new Vector2[4] {Vector2.up, Vector2.right, Vector2.down, Vector2.left};
    public float timeToExplode = 3;
    public float damage = 25;
    [SerializeField] public AnimatedTile[] animatedExplosionTiles;
    [SerializeField] public Tile[] explosionTiles;
    [SerializeField] private LayerMask explosionLayerMask;
    private void Start()
    {
        StartCoroutine(ExplosionTimer());
    }

    public IEnumerator ExplosionTimer()
    {
        yield return new WaitForSeconds(timeToExplode);
        Explode();
    }



    private void Explode()
    {
        foreach (var direction in directions)
        {
            var hit = Physics2D.Raycast(transform.position, direction);
            if(!hit) 
                return;
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                // Переделать под триггер вызрыва
                if(!hit.collider.TryGetComponent(out PlayerController playerController))
                    return;
            }
            
            hit = Physics2D.Raycast(transform.position, direction, 100f, explosionLayerMask);
            
            var hitPos = hit.point;
            hitPos.x = hit.point.x - 0.01f * hit.normal.x;
            hitPos.y = hit.point.y - 0.01f * hit.normal.y;
            
            if(!hit.collider.TryGetComponent(out Tilemap tilemap))
                return;
            
            var bombPosOnTile = tilemap.WorldToCell(transform.position);
            var hitPosOnTile = tilemap.WorldToCell(hitPos);
            
            Debug.DrawRay(transform.position, direction, Color.green, 5f);
            GameAnimationController.Instance.AnimateTileExplosion(hitPosOnTile, bombPosOnTile, explosionTiles, animatedExplosionTiles);
            
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Breakable"))
            {
                tilemap.SetTile(hitPosOnTile, null);
                // Собрать информацию о уничтоженном блоке и отправить на проверку серверу
            }
            
            
        }

        GameController.Instance.Bombs.Remove(this);
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        foreach (var dir in directions)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(new Ray(transform.position, dir * 100));
        }
    }
}
