using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.UIElements;
using System;

public enum IngredientUnit { Spoon, Cup, Bowl, Piece }

[CreateAssetMenu(menuName = "PathTile")]
public class PathTile : Tile
{
    public Sprite DefaultSprite;

    public int someNum;

    [Header("Condition")]
    [SerializeField] bool isConnect_N;
    [SerializeField] bool isConnect_S;
    [SerializeField] bool isConnect_W;
    [SerializeField] bool isConnect_E;
 
    [SerializeField] TileCondition TileCondition;
    [SerializeField] TileCondition[] TileConditions;
 
    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);

        tileData.sprite = DefaultSprite;

        var conditionCode = GetConditionCode();
        foreach(var tile in TileConditions)
        {
            if (tile.TryGetSprite(conditionCode,out Sprite sprite))
            {
                tileData.sprite = sprite;
                break;
            }
        }
    }

    // Helper method to convert conditions to a binary code
    private int GetConditionCode()
    {
        int code = 0;
        code |= (isConnect_N ? 1 : 0) << 3;
        code |= (isConnect_S ? 1 : 0) << 2;
        code |= (isConnect_W ? 1 : 0) << 1;
        code |= (isConnect_E ? 1 : 0);
        return code;
    }
}

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