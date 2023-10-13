using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName ="MazeTile")]
public class TileSelector : ScriptableObject
{
    [Header("DefaultTile")]
    public Tile DefaultTile;

    [Header("Condition")]
    [SerializeField] TileCondition[] TileConditions;

    public Tile GetMacthingTile(TileConnection connection)
    {
        Tile tile = DefaultTile;
        foreach(var condition in TileConditions)
        {
            if (condition.rulePicker.IsMacth(connection))
            {
                tile = condition.Tile;
            }
        }

        return tile;
    }
}
