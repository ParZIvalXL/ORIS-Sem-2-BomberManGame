using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameController : MonoBehaviour
{
    public Queue<Action> actions = new Queue<Action>();
    [SerializeField] public Tilemap ActionTilemap;
    public static GameController Instance;
    
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
