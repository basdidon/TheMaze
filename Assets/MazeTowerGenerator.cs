using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MazeTowerGenerator : MazeGenerator
{
    [field: SerializeField] int TowerFloor { get; set; }
    
    [field: SerializeField] Tile StairTile { get; set; }

    public Vector3Int GetWorldPos(Vector3Int floorPos,int flootIndex) => (Vector3Int.right * flootIndex * (MapRect.xMax + 1))+floorPos;

    public override void PlaceGroundFloor()
    {
        for (int i = 0; i < TowerFloor; i++)
        {
            PlaceGroundFloor(GetWorldPos(Vector3Int.zero,i));
        }
    }


    public void RandomCreateStair(int floorIndex,int otherFloorIndex)
    {
        Vector3Int randCellPos;
        do
        {
            randCellPos = new(Random.Range(MapRect.xMin, MapRect.xMax), Random.Range(MapRect.yMin, MapRect.yMax), 0);
        } 
        while (randCellPos == StartAt);

        GroundTileMap.SetTile(GetWorldPos(randCellPos, floorIndex), StairTile);
        GroundTileMap.SetTile(GetWorldPos(randCellPos, otherFloorIndex), StairTile);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MazeTowerGenerator))]
public class HirachicalMazeGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var towerMazeGenerator = (MazeTowerGenerator)target;

        if (towerMazeGenerator == null) return;

        DrawDefaultInspector();

        if (GUILayout.Button("PlaceGround"))
        {
            towerMazeGenerator.PlaceGroundFloor();
        }

        if (GUILayout.Button("RandomStair"))
        {
            towerMazeGenerator.RandomCreateStair(0,1);
        }

        if (GUILayout.Button("Generate Maze"))
        {
            towerMazeGenerator.CreateMaze();
        }
    }
}
#endif