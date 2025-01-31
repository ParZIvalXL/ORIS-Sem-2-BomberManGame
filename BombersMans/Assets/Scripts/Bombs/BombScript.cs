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
            var hitPos = hit.point;
            hitPos.x = hit.point.x - 0.01f * hit.normal.x + 0.5f;
            hitPos.y = hit.point.y - 0.01f * hit.normal.y + 0.5f;
            if(!hit.collider.TryGetComponent(out Tilemap tilemap))
                return;
            
            var bombPosOnTile = tilemap.WorldToCell(transform.position);
            var hitPosOnTile = tilemap.WorldToCell(hitPos);
            GameAnimationController.Instance.AnimateTileExplosion(hitPosOnTile, bombPosOnTile, explosionTiles, animatedExplosionTiles);
            
            
            Debug.Log("layer: " + (hit.collider.gameObject.layer == LayerMask.NameToLayer("Breakable")) + hitPosOnTile + hit.collider.name);
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Breakable"))
            {
                tilemap.SetTile(tilemap.WorldToCell(hitPosOnTile), null);
                // Собрать информацию о уничтоженном блоке и отправить на проверку серверу
            }

            else if (hit.collider.gameObject.CompareTag("Player"))
            {
                // Переделать под триггер вызрыва
                if(!hit.collider.TryGetComponent(out PlayerController playerController))
                    return;
                playerController.TakeDamage(damage);
            }
            
        }
        
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
