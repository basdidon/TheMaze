using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "PathTile")]
public class PathTile : Tile
{
    Tilemap Tilemap { get; set; }
    readonly Dictionary<Vector3Int, TileConnection> TileConnectionDictionary = new();

    [Header("DefaultSprite")]
    public Sprite DefaultSprite;

    [Header("Condition")]
    [SerializeField] TileCondition[] TileConditions;

    public void Initialize(Tilemap tilemap)
    {
        Tilemap = tilemap;
    }

    public bool CreatePathNode(Vector3Int position)
    {
        if (TileConnectionDictionary.TryAdd(position, new()))
        {
            Tilemap.SetTile(position,this);
            return true;
        }
        return false;
    }

    public void Connect(Vector3Int a, Vector3Int b)
    {
        Debug.Log($"connect {a} to {b}");
        // is adjacent node
        var AToB = b-a;
        if (Mathf.Abs(AToB.x) + Mathf.Abs(AToB.y) != 1)
            return;

        TileConnectionDictionary[a] = SetConnectByDir(TileConnectionDictionary[a],b-a);
        TileConnectionDictionary[b] = SetConnectByDir(TileConnectionDictionary[b],a-b);
    }

    public TileConnection SetConnectByDir(TileConnection connection,Vector3Int dir, bool value = true)
    {
        if (dir == Vector3Int.up)
        {
            connection.isConnectN = value;
        }
        else if (dir == Vector3Int.down)
        {
            connection.isConnectS = value;
        }
        else if (dir == Vector3Int.left)
        {
            connection.isConnectW = value;
        }
        else if (dir == Vector3Int.right)
        {
            connection.isConnectE = value;
        }
        else
        {
            throw new Exception("Unexpected value");
        }

        return connection;
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);

        tileData.sprite = DefaultSprite;

        if(TileConnectionDictionary.TryGetValue(position,out TileConnection connection))
        {
            foreach (var tile in TileConditions)
            {
                if (tile.TryGetSprite(connection, out Sprite sprite))
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