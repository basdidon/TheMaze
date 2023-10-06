using System;
using UnityEngine;
using UnityEngine.Tilemaps;

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

        foreach (var condition in TileConditions)
        {
            //Debug.Log(MazePath?.GetType());

            if (MazePath == null)
                return;

            if (MazePath.TryGetTileConection(position, out TileConnection connection))
            {
                /*
                if (tile.TryGetSprite(connection, out Sprite sprite))
                {
                    tileData.sprite = sprite;
                    break;
                }*/
                if(condition.TryGetTile(connection,out Tile tile))
                {
                    //tileData. = tile;
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