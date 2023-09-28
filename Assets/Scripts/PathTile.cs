using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public struct CellData
{
    public int depth;
    public TileConnection connection;
    
    public CellData SetDepth(int newDepth)
    {
        //Debug.Log($"{depth} -> {newDepth}");
        
        if (depth == -1 || newDepth < depth)
        {
            depth = newDepth;
        }
        return this;
    }

    public CellData SetConnectByDir(Vector3Int dir, bool value = true)
    {
        if (dir == Vector3Int.up)
        {
            connection.IsConnectN = value;
        }
        else if (dir == Vector3Int.down)
        {
            connection.IsConnectS = value;
        }
        else if (dir == Vector3Int.left)
        {
            connection.IsConnectW = value;
        }
        else if (dir == Vector3Int.right)
        {
            connection.IsConnectE = value;
        }
        else
        {
            throw new Exception("Unexpected value");
        }

        return this;
    }
}

[CreateAssetMenu(menuName = "PathTile")]
public class PathTile : Tile
{
    Tilemap Tilemap { get; set; }
    Dictionary<Vector3Int, CellData> CellDataDictionary { get; set; }

    [Header("DefaultSprite")]
    public Sprite DefaultSprite;

    [Header("Condition")]
    [SerializeField] TileCondition[] TileConditions;

    public void Initialize(Dictionary<Vector3Int, CellData> cellDataDict)
    {
        CellDataDictionary = cellDataDict;
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);

        tileData.sprite = DefaultSprite;

        if(CellDataDictionary.TryGetValue(position,out CellData cellData))
        {
            foreach (var tile in TileConditions)
            {
                if (tile.TryGetSprite(cellData.connection, out Sprite sprite))
                {
                    tileData.sprite = sprite;
                    break;
                }
            }
        }
    }
}

/*
#if UNITY_EDITOR

[CustomEditor(typeof(PathTile))]
class TileConditionEditor : Editor
{
    
    public override void OnInspectorGUI()
    {
        var PathTile = (PathTile) target;

        if (PathTile == null) return;

        if (GUILayout.Button("add me :)"))
        {
            PathTile.someNum++;
            Debug.Log(PathTile.someNum);
        }

        DrawDefaultInspector();
    }
}
#endif
*/