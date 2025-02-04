using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace NetCode
{
    public class GameAnimationController : MonoBehaviour
    {
        public static GameAnimationController Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }
        
        public void AnimateTileExplosion(Vector3Int hitPosOnTile, Vector3Int bombPosOnTile, Tile[] explosionTiles, AnimatedTile[] animatedExplosionTiles){
            Vector3Int[] pos = {hitPosOnTile, bombPosOnTile};
            List<Vector3Int> AnimatedTiles = new List<Vector3Int>();
            
            if(hitPosOnTile.y == bombPosOnTile.y)
            {
                var x = pos.Select(s => s.x).ToList();
                x.Sort();
                for (int i = x[0]; i <= x[1]; i++)
                {
                    if(i == hitPosOnTile.x || i == bombPosOnTile.x) continue;
                    var currentCell = new Vector3Int(i, bombPosOnTile.y, 0);
                    GameController.Instance.ActionTilemap.SetTile(currentCell, animatedExplosionTiles[0]);
                    AnimatedTiles.Add(currentCell);
                }
            }
            else
            {
                var y = pos.Select(s => s.y).ToList();
                y.Sort();
                for (int i = y[0]; i <= y[1]; i++)
                {
                    if(i == hitPosOnTile.y || i == bombPosOnTile.y) continue;
                    var currentCell = new Vector3Int(bombPosOnTile.x, i, 0);
                    GameController.Instance.ActionTilemap.SetTile(currentCell, animatedExplosionTiles[0]);
                    GameController.Instance.ActionTilemap
                        .SetTransformMatrix(currentCell,Matrix4x4.Rotate(Quaternion.Euler(0, 0, 90f)));
                    AnimatedTiles.Add(currentCell);
                }
            }
                    
            GameController.Instance.ActionTilemap.SetTile(bombPosOnTile, explosionTiles[0]);
            AnimatedTiles.Add(bombPosOnTile);
            
            StartCoroutine(TileCooldown(AnimatedTiles));
        }

        private IEnumerator TileCooldown(List<Vector3Int> AnimatedTiles)
        {
            yield return new WaitForSeconds(0.2f);
            foreach (var tile in AnimatedTiles)
            {
                GameController.Instance.ActionTilemap.SetTile(tile, null);
            }
        }
    }
}