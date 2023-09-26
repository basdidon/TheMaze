using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "PathTile")]
public class PathTile : Tile
{
    //[Header("DefaultSprite")]
    public Sprite DefaultSprite;

    //[Header("Condition")]
    public TileConnection connection;

    [SerializeField] TileCondition[] TileConditions;
 
    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);

        tileData.sprite = DefaultSprite;

        var conditionCode = GetConditionCode();
        foreach(var tile in TileConditions)
        {
            if (tile.TryGetSprite(connection,out Sprite sprite))
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
        return code;
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