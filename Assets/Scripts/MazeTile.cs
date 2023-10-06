using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName ="MazeTile")]
public class MazeTile : ScriptableObject
{
    [Header("DefaultTile")]
    public Tile DefaultTile;

    [Header("Condition")]
    [SerializeField] TileCondition[] TileConditions;

    public Tile GetTile(TileConnection connection)
    {
        Tile tile = DefaultTile;
        foreach(var condition in TileConditions)
        {
            if (condition.rule4Dir.IsMacth(connection))
            {
                tile = condition.Tile;
            }
        }

        return tile;
    }
}
