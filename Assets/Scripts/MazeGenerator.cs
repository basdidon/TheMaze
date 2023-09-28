using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class MazeGenerator : MonoBehaviour
{
    [field: SerializeField] public Tilemap GroundTileMap { get; private set; }
    [field: SerializeField] public Tilemap PathTileMap { get; private set; }

    [field: SerializeField] public Tile GroundTile { get; private set; }
    [field: SerializeField] public Tile FinishTile { get; private set; }
    [field: SerializeField] public PathTile PathTile { get; private set; }

    [field: SerializeField] public RectInt MapRect { get; private set; }

    [field: SerializeField] public Vector3Int StartAt {get; private set;}
    [field: SerializeField] public Vector3Int EndAt {get; private set;}

    readonly Dictionary<Vector3Int, CellData> CellDataDictionary = new();

    [Header("DefaultSprite")]
    public Sprite DefaultSprite;

    public KeyValuePair<Vector3Int, CellData> MostDepthCell => CellDataDictionary.OrderByDescending(pair => pair.Value.depth).First();


    public bool CreatePathNode(Vector3Int position)
    {
        if (CellDataDictionary.TryAdd(position, new() { depth = -1 }))
        {
            PathTileMap.SetTile(position, PathTile);
            return true;
        }
        return false;
    }

    public void ResetMap(RectInt mapRect, Vector3Int startPos)
    {
        CellDataDictionary.Clear();

        for (int x = mapRect.xMin; x < mapRect.xMax; x++)
        {
            for (int y = mapRect.yMin; y < mapRect.yMax; y++)
            {
                CreatePathNode(new Vector3Int(x, y));
            }
        }

        CellDataDictionary[startPos] = new CellData { depth = 0 };
    }

    public void Connect(Vector3Int from, Vector3Int to)
    {
        var dir = to - from;
        if (Mathf.Abs(dir.x) + Mathf.Abs(dir.y) != 1)
            return;

        var fromCellData = CellDataDictionary[from];
        var toCellData = CellDataDictionary[to];

        CellDataDictionary[from] = fromCellData.SetConnectByDir(to - from);
        CellDataDictionary[to] = toCellData.SetConnectByDir(from - to).SetDepth(fromCellData.depth + 1);
    }

    //
    void Start()
    {
        PathTile.Initialize(CellDataDictionary);
    }

    public virtual void PlaceGroundFloor()
    {
        GroundTileMap.ClearAllTiles();
        PlaceGroundFloor(Vector3Int.zero);
    }

    protected void PlaceGroundFloor(Vector3Int offset)
    {
        for (int x = MapRect.xMin; x < MapRect.xMax; x++)
        {
            for (int y = MapRect.yMin; y < MapRect.yMax; y++)
            {
                var cellPos = new Vector3Int(x, y);
                GroundTileMap.SetTile(cellPos + offset, GroundTile);
            }
        }
    }



    List<Vector3Int> visitedList;
    List<Vector3Int> finishList;

    public void CreateMaze()
    {
        GroundTileMap.SetTile(StartAt, GroundTile);
        GroundTileMap.SetTile(EndAt, GroundTile);
        ResetMap(MapRect,StartAt);

        visitedList = new() { StartAt };
        finishList = new();

        while(visitedList.Count > 0)
        {
            var currentPos = visitedList.Last();
            if(RandomUnvisitNode(currentPos,out Vector3Int result))
            {
                Connect(currentPos, result);
                visitedList.Add(result);
            }
            else
            {
                visitedList.Remove(currentPos);
                finishList.Add(currentPos);
            }
        }

        PathTileMap.RefreshAllTiles();
        EndAt = MostDepthCell.Key;
        //Debug.Log($"MostDethpCell is {EndAt} depth : {PathTile.MostDepthCell.Value.depth}");
        GroundTileMap.SetTile(StartAt,FinishTile);
        GroundTileMap.SetTile(EndAt,FinishTile);
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

        if (GUILayout.Button("PlaceGround"))
        {
            mazeGenerator.PlaceGroundFloor();
        }

        if (GUILayout.Button("Generate Maze"))
        {
            mazeGenerator.CreateMaze();
        }
    }
}
#endif