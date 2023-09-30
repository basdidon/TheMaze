using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public struct CellData
{
    public int? depth;
    public int? SectionId { get; set; }
    public TileConnection connection;

    public CellData SetDepth(int newDepth)
    {
        if (depth == null || newDepth < depth)
        {
            depth = newDepth;
        }
        return this;
    }

    public CellData SetConnectByDir(Vector2Int dir, bool value = true)
    {
        if (dir == Vector2Int.up)
        {
            connection.IsConnectN = value;
        }
        else if (dir == Vector2Int.down)
        {
            connection.IsConnectS = value;
        }
        else if (dir == Vector2Int.left)
        {
            connection.IsConnectW = value;
        }
        else if (dir == Vector2Int.right)
        {
            connection.IsConnectE = value;
        }
        else
        {
            throw new Exception("Unexpected value");
        }

        return this;
    }

    public CellData SetConnectByDir(Vector3Int dir, bool value = true)
    {
        return SetConnectByDir((Vector2Int)dir, value);
    }

    internal CellData SetDepth(int? v)
    {
        throw new NotImplementedException();
    }
}

public interface IMazePath
{
    // put PathTile.Initialize(this) to start
    PathTile PathTile { get; }
    bool TryGetTileConection(Vector3Int cellPos, out TileConnection connection);
}

[CreateAssetMenu(menuName = "PathTile")]
public class PathTile : Tile
{
    IMazePath MazePath { get; set; }

    [Header("DefaultSprite")]
    public Sprite DefaultSprite;

    [Header("Condition")]
    [SerializeField] TileCondition[] TileConditions;

    public void Initialize(IMazePath mazePath)
    {
        MazePath = mazePath;
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);

        tileData.sprite = DefaultSprite;

        foreach (var tile in TileConditions)
        {
            if (MazePath.TryGetTileConection(position, out TileConnection connection))
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