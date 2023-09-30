using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

[Serializable]
public class Floor
{
    public RectInt FloorRect { get; }
    [field: SerializeField] public int NumSection { get; private set; }
    Dictionary<Vector2Int, int> SectionMap { get; }
    Dictionary<Vector2Int, CellData> CellDataMap { get; }
    public KeyValuePair<Vector2Int, CellData> MostDepthCell => CellDataMap.OrderByDescending(pair => pair.Value.depth).First();

    public Floor(RectInt floorRect, int numSection)
    {
        FloorRect = floorRect;
        NumSection = numSection > 0? numSection:1;

        SectionMap = new();
        CellDataMap = new();

        PlaceSectionRoot();
    }

    List<Vector2Int> visitedList;
    List<Vector2Int> finishList;

    void PlaceSectionRoot()
    {
        Vector2Int[] StartAts = new Vector2Int[NumSection];
        for (int i = 0; i < NumSection; i++)
        {
            Vector2Int randPos;
            do
            {
                randPos = new(Random.Range(FloorRect.xMin, FloorRect.xMax), Random.Range(FloorRect.yMin, FloorRect.yMax));
            } while (!SectionMap.TryAdd(randPos,i));
            StartAts[i] = randPos;
        }

        // start create
        visitedList = new(StartAts);
        finishList = new();

        while (visitedList.Count > 0)
        {
            Vector2Int toSearchCell;

            if (NumSection > 1)
            {
                var randSection = Random.Range(0, NumSection);
                toSearchCell = Vector2Int.zero;
                //if (visitedList.any(cell=> SectionMap.v))
                
                
                // ***** endless loop below ****************//
                
                
                /*
                toSearchCell = visitedList.LastOrDefault(cell =>
                {
                    if (SectionMap.TryGetValue(cell, out int value))
                    {
                        return value == randSection;
                    }
                    return false;
                });*/
            }
            else
            {
                toSearchCell = visitedList.Last();
            }

            if (RandomUnvisitNode(toSearchCell, out Vector2Int result))
            {
                Connect(toSearchCell, result);
                visitedList.Add(result);
            }
            else
            {
                visitedList.Remove(toSearchCell);
                finishList.Add(toSearchCell);
            }
        }
    }

    public bool TryGetCellData(Vector2Int cellPos, out CellData cellData) => CellDataMap.TryGetValue(cellPos, out cellData);
    public bool TryGetSection(Vector2Int cellPos,out int sectionIndex) => SectionMap.TryGetValue(cellPos,out sectionIndex);

    public void Connect(Vector2Int from, Vector2Int to)
    {
        var dir = to - from;
        if (Mathf.Abs(dir.x) + Mathf.Abs(dir.y) != 1)
            return;

        if(SectionMap.TryGetValue(from,out int value))
        {
            SectionMap.Add(to, value);
        }

        if (!CellDataMap.TryGetValue(from, out CellData fromCellData))
        {
            fromCellData = new();
            CellDataMap.Add(from, fromCellData);
        }

        if (!CellDataMap.TryGetValue(to, out CellData toCellData))
        {
            toCellData = new();
            CellDataMap.Add(to, toCellData);
        }

        CellDataMap[from] = fromCellData.SetConnectByDir(to - from);
        fromCellData.depth ??= 1;
        if(fromCellData.depth is int fromDepth)
        {
            CellDataMap[to] = toCellData.SetConnectByDir(from - to).SetDepth(fromDepth + 1);
        }
    }

    readonly Vector2Int[] neighborDirs = new Vector2Int[]
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right,
    };

    bool RandomUnvisitNode(Vector2Int pos, out Vector2Int result)
    {
        result = Vector2Int.zero;
        var nodes = neighborDirs.Select(dir => dir + pos)
            .Where(pos => !IsOutOfBound(pos) && !visitedList.Contains(pos) && !finishList.Contains(pos))
            .ToArray();

        if (nodes.Length == 0)
            return false;

        result = nodes[Random.Range(0, nodes.Length)];
        return true;
    }

    bool IsOutOfBound(Vector2Int pos)
    {
        if (pos.x < FloorRect.xMin || pos.x >= FloorRect.xMax)
            return true;
        if (pos.y < FloorRect.yMin || pos.y >= FloorRect.yMax)
            return true;

        return false;
    }
}

public class MazeTowerGenerator : MonoBehaviour, IMazePath
{
    // TileMaps
    [field: SerializeField] public Tilemap GroundTileMap { get; private set; }
    [field: SerializeField] public Tilemap PathTileMap { get; private set; }
    [field: SerializeField] public Tilemap SectionTileMap { get; private set; }

    // Tiles
    [field: SerializeField] public Tile GroundTile { get; private set; }
    [field: SerializeField] public Tile FinishTile { get; private set; }
    [field: SerializeField] public Tile SectionTile { get; private set; }
    [field: SerializeField]
    public Color[] SectionColors { get; private set; } = new Color[] {
        Color.white,
        Color.blue,
        Color.red,
        Color.gray,
    };
    [field: SerializeField] public PathTile PathTile { get; private set; }

    // Floor Properties
    [field: SerializeField] int TowerFloor { get; set; }
    [field: SerializeField] int FloorWidth { get; set; } = 20;
    [field: SerializeField] int FloorHeight { get; set; } = 20;
    [field: SerializeField] int FloorOffset { get; set; } = 1;
    [field: SerializeField] List<Floor> Floors { get; set; }

    private void Start()
    {
        PathTile.Initialize(this);
    }

    // Implement IMazePath
    public bool TryGetTileConection(Vector3Int cellPos,out TileConnection connection)
    {
        connection = new();

        foreach (var floor in Floors)
        {
            // inside rect handle
            if(floor.TryGetCellData((Vector2Int)cellPos,out CellData cellData))
            {
                connection = cellData.connection;
                return true;
            }
        }

        return false;
    }

    void ClearTileMaps()
    {
        GroundTileMap.ClearAllTiles();
        SectionTileMap.ClearAllTiles();
        PathTileMap.ClearAllTiles();
    }

    public void PlaceGroundFloor()
    {
        ClearTileMaps();
        Floors.Clear();

        for (int i = 0; i < TowerFloor; i++)
        {
            var floor = new Floor(new RectInt(i * (FloorWidth + FloorOffset), 0, FloorWidth, FloorHeight), 3);
            Floors.Add(floor);

            foreach (var rectPos in floor.FloorRect.allPositionsWithin)
            {
                Vector3Int cellPos = (Vector3Int) rectPos;

                // Ground
                GroundTileMap.SetTile(cellPos, GroundTile);

                if (floor.TryGetSection(rectPos, out int sectionIdx))
                {
                    Tile tile = SectionTile;
                    if(sectionIdx < SectionColors.Length)
                    {
                        tile.color = SectionColors[sectionIdx];
                    }
                    SectionTileMap.SetTile(cellPos, tile);
                }

                if (floor.TryGetCellData(rectPos, out CellData cellData))
                {
                    PathTileMap.SetTile(cellPos, PathTile);
                }

            }
            
        }
    }
    /*
public void RandomFloorSectionOrder()
{
   if (SectionCount < TowerFloor)
       SectionCount = TowerFloor;

   FloorSectionOrder = new int?[SectionCount];

   for (int i = 0; i < TowerFloor; i++)
   {
       int rand;

       do
       {
           rand = Random.Range(0, SectionCount);
       } while (FloorSectionOrder[rand] != null);

       FloorSectionOrder[rand] = i;
   }

   for (int i = 0; i < SectionCount; i++)
   {
       if (FloorSectionOrder[i] != null)
           continue;

       FloorSectionOrder[i] = Random.Range(0, TowerFloor);
   }

   Debug.Log("--------------");
   foreach (var value in FloorSectionOrder)
   {
       Debug.Log(value);
   }
}
public void RandomCreateStair(int floorIndex, int otherFloorIndex)
{
   int lowerFloorIndex;
   int upperFloorIndex;

   if (floorIndex > otherFloorIndex)
   {
       lowerFloorIndex = otherFloorIndex;
       upperFloorIndex = floorIndex;
   }
   else if (floorIndex < otherFloorIndex)
   {
       lowerFloorIndex = floorIndex;
       upperFloorIndex = otherFloorIndex;
   }
   else  // equals
   {
       return;
   }

   Vector3Int randCellPos;
   do
   {
       randCellPos = new(UnityEngine.Random.Range(MapRect.xMin, MapRect.xMax), Random.Range(MapRect.yMin, MapRect.yMax), 0);
   }
   while (randCellPos == StartAt);

   for (int i = lowerFloorIndex; i <= upperFloorIndex - lowerFloorIndex; i++)
   {
       if (i == upperFloorIndex || i == lowerFloorIndex)
       {
           GroundTileMap.SetTile(GetWorldPos(randCellPos, i), StairTile);
       }
       else
       {
           GroundTileMap.SetTile(GetWorldPos(randCellPos, i), null);
       }
   }
}
*/
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
        /*
        if (GUILayout.Button("RamdomSectionOrder"))
        {
            //towerMazeGenerator.RandomFloorSectionOrder();
        }

        if (GUILayout.Button("RandomStair"))
        {
            //towerMazeGenerator.RandomCreateStair(0,2);
        }

        if (GUILayout.Button("Generate Maze"))
        {
            //towerMazeGenerator.CreateMaze();
        }*/
    }
}
#endif
