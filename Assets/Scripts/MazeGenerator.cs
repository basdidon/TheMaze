using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

public class MazeGenerator : MonoBehaviour
{
    [field: SerializeField] public Tilemap GroundTileMap { get; private set; }
    [field: SerializeField] public Tilemap PathTileMap { get; private set; }

    [field: SerializeField] public Tile GroundTile {get;private set;}
    [field: SerializeField] public PathTile PathTile {get;private set;}

    [field: SerializeField] public RectInt MapRect { get; private set; }

    void Start()
    {
        PathTile.Initialize(PathTileMap);

        for(int x=MapRect.xMin; x < MapRect.xMax; x++)
        {
            for(int y=MapRect.yMin; y< MapRect.yMax; y++)
            {
                var cellPos = new Vector3Int(x, y);
                GroundTileMap.SetTile(cellPos,GroundTile);
                PathTile.CreatePathNode(cellPos);
            }
        }
    }

    List<Vector3Int> visitedList;
    List<Vector3Int> finishList;

    public void CreateMaze(Vector3Int startPos)
    {
        visitedList = new() { startPos };
        finishList = new();

        while(visitedList.Count > 0)
        {
            var currentPos = visitedList.Last();
            if(RandomUnvisitNode(currentPos,out Vector3Int result))
            {
                PathTile.Connect(currentPos, result);
                PathTileMap.RefreshAllTiles();
                visitedList.Add(result);
            }
            else
            {
                visitedList.Remove(currentPos);
                finishList.Add(currentPos);
            }
        }
    }

    public void CreateMazeButAnimate(Vector3Int startPos)
    {
        visitedList = new() { startPos };
        finishList = new();

        StartCoroutine(RandomNextNodeRoutine());
    }

    IEnumerator RandomNextNodeRoutine()
    {
        var currentPos = visitedList.Last();
        if (RandomUnvisitNode(currentPos, out Vector3Int result))
        {
            PathTile.Connect(currentPos, result);
            PathTileMap.RefreshAllTiles();
            visitedList.Add(result);
        }
        else
        {
            visitedList.Remove(currentPos);
            finishList.Add(currentPos);
        }

        if (visitedList.Count > 0)
        {
            yield return RandomNextNodeRoutine();
        }
        else
        {
            yield return null;
        }
    }

    readonly Vector3Int[] neighborDirs = new Vector3Int[]
    {
        Vector3Int.up,
        Vector3Int.down,
        Vector3Int.left,
        Vector3Int.right,
    };

    bool RandomUnvisitNode(Vector3Int pos,out Vector3Int result)
    {
        result = Vector3Int.zero;
        var nodes = neighborDirs.Select(dir=> dir+pos)
            .Where(pos => !IsOutOfBound(pos) && !visitedList.Contains(pos) && !finishList.Contains(pos))
            .ToArray();
        
        if (nodes.Length == 0)
            return false;

        result = nodes[Random.Range(0, nodes.Length)];
        return true;
    }

    bool IsOutOfBound(Vector3Int pos)
    {
        if (pos.x < MapRect.xMin || pos.x >= MapRect.xMax)
            return true;
        if (pos.y < MapRect.yMin || pos.y >= MapRect.yMax)
            return true;
        Debug.Log($"{MapRect.xMin} < {pos.x} < {MapRect.xMax}");
        Debug.Log($"{MapRect.yMin} < {pos.y} < {MapRect.yMax}");
        return false;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MazeGenerator))]
public class MazeGeneratorEditor: Editor
{
    public override void OnInspectorGUI()
    {
        var mazeGenerator = (MazeGenerator)target;

        if (mazeGenerator == null) return;

        DrawDefaultInspector();

        if (GUILayout.Button("Generate Maze"))
        {
            mazeGenerator.CreateMaze(Vector3Int.zero);
        }
        if (GUILayout.Button("Generate Maze (Animated) don't "))
        {
            mazeGenerator.CreateMazeButAnimate(Vector3Int.zero);
        }
    }
}
#endif