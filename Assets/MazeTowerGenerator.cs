using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Linq;
using Random = UnityEngine.Random;

[Serializable]
public class Floor
{
    RectInt MapRect { get; set; }
    [field: SerializeField] public int NumSection { get; private set; }
    [SerializeField] int?[,] sectionMap;

    readonly Dictionary<Vector3Int, CellData> CellDataDictionary = new();
    public KeyValuePair<Vector3Int, CellData> MostDepthCell => CellDataDictionary.OrderByDescending(pair => pair.Value.depth).First();

    public Floor(RectInt mapSize,int numSection)
    {
        MapRect = mapSize;
        NumSection = numSection;

        sectionMap = new int?[MapRect.width, MapRect.height];
        //GenerateSectionMap(GenerateNoise());
        PlaceSectionRoot();
    }

    List<Vector3Int> visitedList;
    List<Vector3Int> finishList;

    void PlaceSectionRoot()
    {
        Vector3Int[] StartAts = new Vector3Int[NumSection];
        for (int i = 0; i < NumSection; i++)
        {
            Vector2Int randPos;
            do
            {
                randPos = new(Random.Range(MapRect.xMin, MapRect.xMax), Random.Range(MapRect.yMin, MapRect.yMax));
            } while (sectionMap[randPos.x, randPos.y] != null);

            sectionMap[randPos.x, randPos.y] = i;
            StartAts[i] = new Vector3Int(randPos.x,randPos.y);
        }

        // start create
        visitedList = new(StartAts);
        finishList = new();

        while (visitedList.Count > 0)
        {
            var currentPos = visitedList.Last();
            if (RandomUnvisitNode(currentPos, out Vector3Int result))
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
    }
    public int? GetSection(Vector2Int cellPos) => sectionMap[cellPos.x,cellPos.y];

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

    readonly Vector3Int[] neighborDirs = new Vector3Int[]
    {
        Vector3Int.up,
        Vector3Int.down,
        Vector3Int.left,
        Vector3Int.right,
    };

    bool RandomUnvisitNode(Vector3Int pos, out Vector3Int result)
    {
        result = Vector3Int.zero;
        var nodes = neighborDirs.Select(dir => dir + pos)
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
    /*
    public float[,] GenerateNoise()
    {
        float[,] noiseMap = new float[MapRect.width,MapRect.height];
        
        for(int x = 0; x < noiseMap.GetLength(0); x++)
        {
            for (int y = 0; y < noiseMap.GetLength(1); y++)
            {
                noiseMap[x, y] = Mathf.Clamp01(Mathf.PerlinNoise(x / (float) MapRect.width, y / (float) MapRect.height));
                Debug.Log($" noiseMap[{x},{y}] : {noiseMap[x, y]}");
            }
        }

        return noiseMap;
    }


    void GenerateSectionMap(float[,] noiseMap)
    {
        // Random Section Size
        int[] sectionsSize = new int[NumSection];
        for (int i = 0; i < sectionsSize.Length; i++)
        {
            sectionsSize[i] = Random.Range(1, 4);
        }

        int sum = sectionsSize.Sum();
        float[] normalizedArr = new float[NumSection];
        for (int i = 0; i < normalizedArr.Length; i++)
        {
            normalizedArr[i] = (float) sectionsSize[i] / sum;
            Debug.Log($"{normalizedArr[i]} = {sectionsSize[i]} / {sum}");
        }

        float[] cumulativeArr = new float[NumSection];
        for (int i = 0; i < cumulativeArr.Length; i++)
        {
            if (i == 0)
            {
                cumulativeArr[i] = normalizedArr[i] ;
            }
            else
            {
                cumulativeArr[i] = cumulativeArr[i - 1] + normalizedArr[i];
            }

            Debug.Log($"cumulativeArr[{i}] : {cumulativeArr[i]}");
        }

        sectionMap = new int?[MapRect.width,MapRect.height];
        // set section to map
        for (int x = 0; x < sectionMap.GetLength(0); x++)
        {
            for (int y = 0; y < sectionMap.GetLength(1); y++)
            {
                for(int i = 0; i < cumulativeArr.Length; i++)
                {
                    if(noiseMap[x,y] < cumulativeArr[i])
                    {
                        sectionMap[x, y] = i;
                        break;
                    }
                }
            }
        }
    }*/
}

public class MazeTowerGenerator : MazeGenerator
{
    [field: SerializeField] int TowerFloor { get; set; }
    [field: SerializeField] List<Floor> Floors { get; set; }
    [field: SerializeField] Tile StairTile { get; set; }
    [field: SerializeField] int SectionCount { get; set; }
    [field: SerializeField] int?[] FloorSectionOrder { get; set; }

    [field: SerializeField] Tilemap SectionTileMap { get; set; }
    [field: SerializeField] Tile section1Tile { get; set; }
    [field: SerializeField] Tile section2Tile { get; set; }

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

    public Vector3Int GetWorldPos(Vector3Int floorPos, int flootIndex) => (Vector3Int.right * flootIndex * (MapRect.xMax + 1)) + floorPos;

    public override void PlaceGroundFloor()
    {
        for (int i = 0; i < TowerFloor; i++)
        {
            PlaceGroundFloor(GetWorldPos(Vector3Int.zero,i));
            Floors.Add(new(MapRect, 2));
        }

        for(int i =0; i < TowerFloor; i++)
        {
            for (int x = MapRect.xMin; x < MapRect.xMax; x++)
            {
                for (int y = MapRect.yMin; y < MapRect.yMax; y++)
                {
                    var cellPos = new Vector3Int(x, y);
                    //Debug.Log(Floors[i].GetSection(new Vector2Int(cellPos.x, cellPos.y)));
                    Tile tile = Floors[i].GetSection(new Vector2Int(cellPos.x, cellPos.y)) switch
                    {
                        null => null,
                        0 => section1Tile,
                        1 => section2Tile,
                        _ => null,
                    };
                    SectionTileMap.SetTile(cellPos + GetWorldPos(Vector3Int.zero,i), tile);
                }
            }
        }
    }

    public void RandomCreateStair(int floorIndex,int otherFloorIndex)
    {
        int lowerFloorIndex;
        int upperFloorIndex;
        
        if (floorIndex > otherFloorIndex)
        {
            lowerFloorIndex = otherFloorIndex;
            upperFloorIndex = floorIndex;
        }
        else if(floorIndex < otherFloorIndex)
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

        for(int i = lowerFloorIndex; i <= upperFloorIndex - lowerFloorIndex; i++)
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

        if (GUILayout.Button("RamdomSectionOrder"))
        {
            towerMazeGenerator.RandomFloorSectionOrder();
        }

        if (GUILayout.Button("RandomStair"))
        {
            towerMazeGenerator.RandomCreateStair(0,2);
        }

        if (GUILayout.Button("Generate Maze"))
        {
            towerMazeGenerator.CreateMaze();
        }
    }
}
#endif